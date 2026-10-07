var builder = WebApplication.CreateBuilder(args);

// ==========================================
// CORS - Permite conexion desde Angular
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ==========================================
// Dependencias de la aplicacion
// ==========================================
builder.Services.AddScoped<Commerce.Api.Data.DatabaseConnection>();
builder.Services.AddScoped<Commerce.Api.Repositories.CommerceRepository>();


// ==========================================
// Controllers
// ==========================================
builder.Services.AddControllers();


// ==========================================
// OpenAPI
// ==========================================
builder.Services.AddOpenApi();


var app = builder.Build();


// ==========================================
// OpenAPI solo en desarrollo
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


// ==========================================
// CORS
// Debe ir antes de MapControllers
// ==========================================
app.UseCors("AngularApp");


app.UseAuthorization();


// ==========================================
// Endpoints
// ==========================================
app.MapControllers();


app.Run();