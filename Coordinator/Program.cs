using Coordinator.Contexts;
using Coordinator.Services;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TwoPhaseCommitContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MSSQLServer")));

builder.Services.AddHttpClient("OrderAPI", client => client.BaseAddress = new("https://localhost:7087/"));
builder.Services.AddHttpClient("PaymentAPI", client => client.BaseAddress = new("https://localhost:7293/"));
builder.Services.AddHttpClient("StockAPI", client => client.BaseAddress = new("https://localhost:7127/"));

builder.Services.AddSingleton<ITransactionService, TransactionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/create-order-transaction", async (ITransactionService _transactionService) =>
{
    //Phase 1 - Prepare
    var transactionId = await _transactionService.CreateTransactionAsync();
    await _transactionService.PrepareServicesAsync(transactionId);
    bool transactionState = await _transactionService.CheckReadyServicesAsync(transactionId);

    if (transactionState)
    {
        //Phase 2 - Commit
        await _transactionService.CommitAsync(transactionId);
        transactionState =  await _transactionService.CheckTransactionStateServicesAsync(transactionId);

        if (!transactionState)
            await _transactionService.RollbackAsync(transactionId);
    }

});

app.Run();
