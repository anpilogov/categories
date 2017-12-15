using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CategorieaApi.Models
{
    public class Response
    {
        public int[] Categories { get; set; }
        public string[] Names { get; set; }
        public string Text { get; set; }
    }
}