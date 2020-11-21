using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Data;
using WebAPI.Models;
using WebAPI.Hateoas;
using System.Collections.Generic;

namespace WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext _database;
        private Hateoas.Hateoas _hateoas;

        public ProdutosController(ApplicationDbContext database)
        {
            _database = database;
            _hateoas = new Hateoas.Hateoas("localhost:5001/api/v1/produtos");
            _hateoas.AddAction("get_info", "get");
            _hateoas.AddAction("delete_product", "delete");
            _hateoas.AddAction("edit_product", "patch");
        }

        [HttpGet]
        public IActionResult ListarProduto()
        {
            var produtos = _database.Produtos.ToList();
            List<ProdutoContainer> produtosHateoas = new List<ProdutoContainer>();

            foreach (var prod in produtos) {
                ProdutoContainer produtoHateoas = new ProdutoContainer();
                produtoHateoas.produto = prod;
                produtoHateoas.links = _hateoas.GetAction(prod.Id.ToString());
                produtosHateoas.Add(produtoHateoas);
            }

            return Ok(produtosHateoas);
        }

        [HttpGet("{id}")]
        public IActionResult ListarProdutoId(int id)
        {
            try {
                var produtos = _database.Produtos.First(p => p.Id == id);
                ProdutoContainer produtoHateoas = new ProdutoContainer();
                produtoHateoas.produto = produtos;
                produtoHateoas.links = _hateoas.GetAction(produtos.Id.ToString());
                return Ok(produtoHateoas);
            }
            catch {
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeletarProduto(int id)
        {
            try {
            Produto produtos = _database.Produtos.First(p => p.Id == id);
            _database.Produtos.Remove(produtos);
            _database.SaveChanges();
            return Ok();
            }
            catch {
                return new ObjectResult("");
            }
        }

        [HttpPost]
        public IActionResult CriarProduto([FromBody] ProdutoTemporario produtoTemporario)
        {
            if (produtoTemporario.Preco <= 0) {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Preço deve ser maior que 0"});
            }

            if (produtoTemporario.Nome.Length <= 1 || produtoTemporario.Nome == null) {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Nome deve ter mais que 1 caracter"});
            }

            Produto produto = new Produto();
            produto.Nome = produtoTemporario.Nome;
            produto.Preco = produtoTemporario.Preco;

            _database.Produtos.Add(produto);
            _database.SaveChanges();
            
            Response.StatusCode = 201;
            return new ObjectResult("");
            // return Ok(new {msg = "Muito bão, produto criado com sucesso"});
        }

        [HttpPatch]
        public IActionResult EditarProduto([FromBody] Produto produto)
        {
            if (produto.Id > 0) {
                try {
                    var produtos = _database.Produtos.First(p => p.Id == produto.Id);
                    
                    if (produto != null) {
                        produtos.Nome = produto.Nome != null ? produto.Nome : produtos.Nome;
                        produtos.Preco = produto.Preco != 0 ? produto.Preco : produtos.Preco;

                        _database.SaveChanges();

                        return Ok();
                    }
                    else {
                        return new ObjectResult("Produto não encontrado");
                    }
                }
                catch {
                    return new ObjectResult("Produto não encontrado");
                }
            }
            else {
                Response.StatusCode = 400;
                return new ObjectResult(new {msg = "Id inválido"});
            }
            
        }

        public class ProdutoTemporario
        {
            public string Nome { get; set; }
            public double Preco { get; set; }
        }

        public class ProdutoContainer
        {
            public Produto produto { get; set; }
            public Link[] links { get; set; }
        }
    }
}