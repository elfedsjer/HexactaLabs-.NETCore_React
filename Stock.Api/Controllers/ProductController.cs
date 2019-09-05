﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Stock.Api.DTOs;
using Stock.AppService.Services;
using Stock.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Stock.Api.Extensions;

namespace Stock.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService service;
        private readonly ProductTypeService productTypeService;
        private readonly IMapper mapper;

        public ProductController(ProductService service, ProductTypeService productTypeService, IMapper mapper)
        {
            this.service = service;
            this.productTypeService = productTypeService;
            this.mapper = mapper;
        }

        /// <summary>
        /// Permite recuperar todas las instancias
        /// </summary>
        /// <returns>Una colección de instancias</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ProductDTO>> Get()
        {
            try
            {
                var result = this.service.GetAll();
                var resultDto = this.mapper.Map<IEnumerable<ProductDTO>>(result).ToList();
                foreach (var item in resultDto)
                {
                    AssignProductTypetoProduct(item);
                }

                return resultDto;
            }
            catch(Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Permite recuperar una instancia mediante un identificador
        /// </summary>
        /// <param name="id">Identificador de la instancia a recuperar</param>
        /// <returns>Una instancia</returns>
        [HttpGet("{id}")]
        public ActionResult<ProductDTO> Get(string id)
        {
            var resultDto = this.mapper.Map<ProductDTO>(this.service.Get(id));
            AssignProductTypetoProduct(resultDto);

            return resultDto;
        }

        /// <summary>
        /// Permite crear una nueva instancia
        /// </summary>
        /// <param name="value">Una instancia</param>
        [HttpPost]
        public ProductDTO Post([FromBody] ProductDTO value)
        {
            TryValidateModel(value);
            var product = this.mapper.Map<Product>(value);
            //product.ProductType = this.productTypeService.Get(value.ProductTypeId.ToString());
            var result = this.service.Create(product);
            var resultDto = this.mapper.Map<ProductDTO>(this.service.Create(product));
            AssignProductTypetoProduct(resultDto);          
            
            return resultDto;
        }

        /// <summary>
        /// Permite editar una instancia
        /// </summary>
        /// <param name="id">Identificador de la instancia a editar</param>
        /// <param name="value">Una instancia con los nuevos datos</param>
        [HttpPut("{id}")]
        public void Put(string id, [FromBody] ProductDTO value)
        {
            var product = this.service.Get(id);
            TryValidateModel(value);
            this.mapper.Map<ProductDTO, Product>(value, product);
           // product.ProductType = this.productTypeService.Get(value.ProductTypeId.ToString());
            this.service.Update(product);
        }

        [HttpPost("search")]
        public ActionResult Search([FromBody] ProductSearchDTO model)
        {
            Expression<Func<Product, bool>> filter = x => !string.IsNullOrWhiteSpace(x.Id);

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                filter = filter.AndOrCustom(
                    x => x.Name.ToUpper().Contains(model.Name.ToUpper()),
                    model.Condition.Equals(ActionDto.AND));
            }

            if(!string.IsNullOrWhiteSpace(model.Brand))
            {
                filter = filter.AndOrCustom(
                    x => x.ProductTypeId.ToUpper().Equals(model.Brand.ToUpper()),
                    model.Condition.Equals(ActionDto.AND));
            }

            var products = this.service.Search(filter);

            var resultDto = this.mapper.Map<IEnumerable<ProductDTO>>(products).ToList(); 

            foreach (var item in resultDto)
            {
                AssignProductTypetoProduct(item);
            }

            return Ok(resultDto);
        }

        /// <summary>
        /// Permite borrar una instancia
        /// </summary>
        /// <param name="id">Identificador de la instancia a borrar</param>
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var product = this.service.Get(id);
            this.service.Delete(product);
        }

        /// <summary>
        /// Permite conocer el stock de un producto
        /// </summary>
        /// <param name="id">Identificador del producto</param>
        /// <returns>El stock disponible</returns>
        [HttpGet("stock/{id}")]
        public ActionResult<GenericResultDTO<int>> ObtenerStock(string id)
        {
            return new GenericResultDTO<int>(this.service.ObtenerStock(id));
        }

        /// <summary>
        /// Permite descontar una cantidad de stock a un producto
        /// </summary>
        /// <param name="id">Identificador del producto</param>
        /// <param name="value">La cantidad a descontar</param>
        [HttpPut("stock/descontar/{id}")]
        public void DescontarStock(string id, [FromBody] int value)
        {
            this.service.DescontarStock(id, value);
        }

        /// <summary>
        /// Permite sumar una cantidad de stock a un producto
        /// </summary>
        /// <param name="id">Identificador del producto</param>
        /// <param name="value">La cantidad a sumar</param>
        [HttpPut("stock/sumar/{id}")]
        public void SumarStock(string id, [FromBody] int value)
        {
            this.service.SumarStock(id, value);
        }

        /// <summary>
        /// Permite obtener el precio de venta al público de un producto
        /// </summary>
        /// <param name="id">Identificador del producto</param>
        /// <returns>El precio de venta al público</returns>
        [HttpGet("precioVenta/{id}")]
        public ActionResult<GenericResultDTO<decimal>> ObtenerPrecioVenta(string id)
        {
            return new GenericResultDTO<decimal>(this.service.ObtenerPrecioVentaPublico(id));
        }

        /// <summary>
        /// Permite obtener el precio de venta de un producto para un empleado
        /// </summary>
        /// <param name="id">Identificador del producto</param>
        /// <returns>El precio de venta para un empleado</returns>
        [HttpGet("precioVentaEmpleado/{id}")]
        public ActionResult<GenericResultDTO<decimal>> ObtenerPrecioVentaEmpleado(string id)
        {
            return new GenericResultDTO<decimal>(this.service.ObtenerPrecioVentaEmpleado(id));
        }

        private void AssignProductTypetoProduct(ProductDTO dtoItem)
        {
             if (!string.IsNullOrWhiteSpace(dtoItem.ProductTypeId))
            {
                var productType = this.productTypeService.Get(dtoItem.ProductTypeId);
                dtoItem.ProductTypeId = productType.Id;
                dtoItem.ProductTypeDesc = productType.Description;
                dtoItem.ProcutTypeinitials = productType.Initials;
            }   
        }
    }
}
