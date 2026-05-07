using Microsoft.EntityFrameworkCore;
using ST10484350_CLDV_Part_1_EventEase.Data;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Azurite Registration [cite: 6, 18]
builder.Services.AddScoped(_ =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage")));
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureStorage:blobServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureStorage:queueServiceUri"]!).WithName("AzureStorage");
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureStorage:tableServiceUri"]!).WithName("AzureStorage");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();