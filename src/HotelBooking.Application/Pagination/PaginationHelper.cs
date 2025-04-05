using HotelBooking.Application.DTO.Booking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelBooking.Application.Pagination
{
    public class PaginationHelper
    {
        public static async Task<PaginatedResponse<T>> CreateAsync<T>(
           IQueryable<T> source,
           int pageIndex,
           int pageSize)
        {
            // Validate parameters
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;  // Maximum page size limit

            var totalCount = await source.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Adjust page index if it exceeds total pages
            if (pageIndex > totalPages && totalCount > 0)
            {
                pageIndex = totalPages;
            }

            var items = await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<T>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
        public static Microsoft.AspNetCore.Mvc.IActionResult CreatePaginatedResponse<T>(
            Microsoft.AspNetCore.Mvc.ControllerBase controller,
            PaginatedResponse<T> paginatedList,
            string routeName = null,
            object routeValues = null)
        {
            // Add pagination headers
            var paginationMetadata = new
            {
                totalCount = paginatedList.TotalCount,
                pageSize = paginatedList.PageSize,
                currentPage = paginatedList.PageIndex,
                totalPages = paginatedList.TotalPages,
                hasPrevious = paginatedList.HasPreviousPage,
                hasNext = paginatedList.HasNextPage
            };

            controller.Response.Headers.Add("X-Pagination",
                System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

            // Add links if route name is provided
            if (!string.IsNullOrEmpty(routeName) && routeValues != null)
            {
                var links = new Dictionary<string, string>();

                if (paginatedList.HasPreviousPage)
                {
                    links.Add("prevPage", controller.Url.Link(routeName,
                        new { pageIndex = paginatedList.PageIndex - 1, pageSize = paginatedList.PageSize }));
                }

                if (paginatedList.HasNextPage)
                {
                    links.Add("nextPage", controller.Url.Link(routeName,
                        new { pageIndex = paginatedList.PageIndex + 1, pageSize = paginatedList.PageSize }));
                }

                links.Add("firstPage", controller.Url.Link(routeName,
                    new { pageIndex = 1, pageSize = paginatedList.PageSize }));

                links.Add("lastPage", controller.Url.Link(routeName,
                    new { pageIndex = paginatedList.TotalPages, pageSize = paginatedList.PageSize }));

                controller.Response.Headers.Add("X-Pagination-Links",
                    System.Text.Json.JsonSerializer.Serialize(links));
            }

            return controller.Ok(paginatedList.Items);
        }

        internal class PaginatedResponse<T> : Pagination.PaginatedResponse<BookingDTO>
        {
            public List<BookingDTO> Items { get; set; }
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
