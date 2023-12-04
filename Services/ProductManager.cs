﻿using Microsoft.EntityFrameworkCore;
using DataAccess;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ServiceContracts.DTO.Product;
using ServiceContracts.DTO.Image;
using ServiceContracts.DTO.Feature;
using ServiceContracts;

namespace Services
{
    public sealed class ProductManager : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductManager(ApplicationDbContext context) => _context = context;
        public async Task<bool> IsExistAsync(int id)
        {
            return await _context.Products.AnyAsync(e => e.Id == id);
        }
        public async Task<ProductResponse> GetProductAsync(int id, bool includeType = true, bool includeFeatures = true, bool includeImages = true, bool includeComments = true)
        {
            var context = _context.Products.Where(x => x.Id == id);
            if (includeType)
                await context.Include(x => x.Type).ToListAsync();
            if (includeFeatures)
                await context.Include(x => x.Features).ToListAsync();
            if (includeImages)
                await context.Include(x => x.Images).ToListAsync();
            if (includeComments)
                await context.Include(x => x.Comments).ToListAsync();
            var product = await context.FirstOrDefaultAsync() ?? throw new ArgumentException($"Not possible to find a product by id:{id}", nameof(id));
            return product.ToProductResponse();
        }
        public async Task<List<ProductResponse>> GetProductsAsync(bool includeType = true, bool includeFeatures = true, bool includeImages = true, bool includeComments = true)
        {
            var context = _context.Products;
            if (includeType)
                await context.Include(x => x.Type).ToListAsync();
            if (includeFeatures)
                await context.Include(x => x.Features).ToListAsync();
            if (includeImages)
                await context.Include(x => x.Images).ToListAsync();
            if (includeComments)
                await context.Include(x => x.Comments).ToListAsync();
            return (await context.ToListAsync()).Select(x => x.ToProductResponse()).ToList();
        }
        public async Task CreateAsync(ProductAddRequest product)
        {
            await _context.Products.AddAsync(product.ToProduct());
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(ProductUpdateRequest product)
        {
            var dbProduct = product.ToProduct();
            foreach (var item in product.Images)
            {
                dbProduct.Images.Add(await _context.ProductImages.FindAsync(item.Id));
            }
            foreach (var item in product.Features)
            {
                dbProduct.Features.Add(await _context.Features.FindAsync(item.FeatureId));
            }
            _context.Products.Update(dbProduct);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(ProductResponse product)
        {
            _context.Products.Remove(await _context.Products.FindAsync(product.Id) ?? throw new ArgumentException("Invalid product response"));
            await _context.SaveChangesAsync();
        }
        public async Task DeleteByIdAsync(int id)
        {
            var product = await GetProductAsync(id);
            await DeleteAsync(product);
        }
    }
}
