using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LLII_Systems.Models
{
    public class PaginationModel
    {
        public List<string> Data { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public PaginationModel(List<string> data, int pageSize, int currentPage)
        {
            Data = data;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(data.Count / (double)pageSize);
        }

        public List<string> GetPaginatedData()
        {
            int startIndex = (CurrentPage - 1) * PageSize;
            return Data.Skip(startIndex).Take(PageSize).ToList();
        }
    }
}