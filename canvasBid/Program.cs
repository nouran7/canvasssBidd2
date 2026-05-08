using canvasBid.Data;
using canvasBid.Models;
using canvasBid.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================= SERVICES =================

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// DbContext
builder.Services.AddDbContext<dbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<dbContext>()
    .AddDefaultTokenProviders();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// Authorization
builder.Services.AddAuthorization();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// ================= ROLE SEEDING =================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Admin", "Artist", "Buyer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// ================= PIPELINE =================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BidHub>("/bidHub");//to test signalIr ya raaaaaaaaaaaab b2a t3bttt 
app.Run();
//ana brdo mknsh ynf3 ashw7 b2idy kda w a2ol laaaaaaaaaaaaaaaaaa ana cs kant tt2t3 edy el shw7t bihaa
//bs eh r2yko ya wlad fi el nzam 7aga wla arw3 s7??
// 5lina mtf2een ahm mn el sho8l tasbeet el sho8l ahhh y3ni momken yb2a code zft bs thanks god 3amla comments w m5lya shklo 7lww
// w el b3tl feh bs2l chatgbt ana msh developer off y77 mn google y3ni sba7 el 5eer