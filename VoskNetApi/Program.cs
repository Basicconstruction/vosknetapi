using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Rewrite;
using VoskNetApi.Services;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null; // 保持属性名大小写不变
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All); // 支持所有Unicode字符编码
});

builder.Services.AddSingleton<ITextRecognizeService, TextRecognizeService>();
builder.Services.AddSingleton<IAudioConvertService, AudioConvertService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VoskNetApi", Version = "v1" });
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("CorsPolicy", builder =>
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoskNetApi v1"));
}

app.UseCors("CorsPolicy");

app.UseRouting();

app.UseAuthorization();

var rewriteOptions = new RewriteOptions();
rewriteOptions.AddRedirect("^$", "/swagger/index.html");
app.UseRewriter(rewriteOptions);

app.MapControllers();

app.Run();