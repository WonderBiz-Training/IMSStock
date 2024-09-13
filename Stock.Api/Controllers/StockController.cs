using Microsoft.AspNetCore.Mvc;
using Stock.Application.DTOs;
using Stock.Application.Interfaces;
using Stock.Application.Exceptions;
using System;
using System.Threading.Tasks;

namespace Stock.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockService _stockService;

        public StockController(IStockService stockService)
        {
            _stockService = stockService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var stocks = await _stockService.GetAllStocksAsync();
                return Ok(stocks);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var stock = await _stockService.GetStockByIdAsync(id);
                return Ok(stock);
            }
            catch (StockNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockDto dto)
        {
            try
            {
                var result = await _stockService.CreateStockAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.LocationId }, result);
            }
            catch (StockValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (StockAlreadyExistsException ex)
            {
                return Conflict(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id?}")]
        public async Task<IActionResult> Update(Guid? id, [FromBody] UpdateStockDto dto)
        {
            try
            {
                if (id.HasValue)
                {
                    dto.Id = id.Value;
                }

                var result = await _stockService.UpdateStockAsync(dto);
                return Ok(result);
            }
            catch (StockValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (StockAlreadyExistsException ex)
            {
                return Conflict(ex.Message);
            }
            catch (StockNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _stockService.DeleteStockAsync(id);
                if (!result)
                {
                    return NotFound($"No stock found for id: {id}");
                }
                return NoContent();
            }
            catch (StockNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ApplicationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
