using System.Text.Json.Serialization;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Rag;
using CleanArchitecture.Infrastructure.Repositories;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (string.IsNullOrWhiteSpace(connectionString))
        options.UseInMemoryDatabase("CleanArchitectureDb");
    else
        options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var elasticsearchUrl = Environment.GetEnvironmentVariable("ES_URL")
    ?? builder.Configuration[$"{RagOptions.SectionName}:ElasticsearchUrl"]
    ?? RagOptions.DefaultElasticsearchUrl;

builder.Services.Configure<RagOptions>(builder.Configuration.GetSection(RagOptions.SectionName));
builder.Services.PostConfigure<RagOptions>(options =>
{
    options.ElasticsearchUrl = Environment.GetEnvironmentVariable("ES_URL") ?? options.ElasticsearchUrl;
    options.OllamaUrl = Environment.GetEnvironmentVariable("OLLAMA_URL") ?? options.OllamaUrl;
    options.EmbeddingModel = Environment.GetEnvironmentVariable("DOCRAG_EMBED_MODEL") ?? options.EmbeddingModel;
    options.ChatModel = Environment.GetEnvironmentVariable("DOCRAG_LLM_MODEL") ?? options.ChatModel;
});
builder.Services.AddHttpClient(RagHttpClientNames.Ollama, client => client.Timeout = TimeSpan.FromMinutes(5));
builder.Services.AddSingleton(new ElasticsearchClient(new Uri(elasticsearchUrl)));
builder.Services.AddScoped<ITextChunker, ParagraphTextChunker>();
builder.Services.AddScoped<IDocumentFileReader, LocalDocumentFileReader>();
builder.Services.AddScoped<IEmbeddingGenerator, OllamaEmbeddingGenerator>();
builder.Services.AddScoped<IChunkIndex, ElasticsearchChunkIndex>();
builder.Services.AddScoped<IAnswerGenerator, OllamaAnswerGenerator>();
builder.Services.AddScoped<IDocumentRagService, DocumentRagService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.Run();
