using Study.Models;
using Study.Services;
using Microsoft.AspNetCore.Mvc;

namespace Study.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountsService;

    public AccountController(AccountService accountsService) =>
        _accountsService = accountsService;

    [HttpGet]
    public async Task<List<Account>> Get() =>
        await _accountsService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Account>> Get(int id)
    {
        var account = await _accountsService.GetAsync(id);

        if (account is null)
        {
            return NotFound();
        }

        return account;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Account newAccount)
    {
        await _accountsService.CreateAsync(newAccount);

        return CreatedAtAction(nameof(Get), new { id = newAccount.Id }, newAccount);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(int id, Account updatedAccount)
    {
        var book = await _accountsService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        updatedAccount.Id = book.Id;

        await _accountsService.UpdateAsync(id, updatedAccount);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(int id)
    {
        var book = await _accountsService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _accountsService.RemoveAsync(id);

        return NoContent();
    }
}