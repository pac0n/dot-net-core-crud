using Unoamuchos.Models;
using Unoamuchos.Repository.Contract;
using Unoamuchos.Repository.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IBillRepository<BillDetail>, BillRepository>();
builder.Services.AddScoped<IItemRepository<Items>, ItemRepository>();
builder.Services.AddScoped<IData, Data>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Ebill}/{action=Index}/{id?}");

app.Run();
