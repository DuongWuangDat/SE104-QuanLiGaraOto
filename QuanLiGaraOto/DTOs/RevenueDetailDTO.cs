using System;
using System.Collections.Generic;

namespace QuanLiGaraOto.DTOs
{

    
    public class RevenueDetailDTO
    {
        public int RevenueId { get; set; }
        public int BrandCarId { get; set; }
        public Nullable<int> RepairCount { get; set; }
        public Nullable<double> Ratio { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
    
        public virtual BrandCarDTO BrandCar { get; set; }
        public virtual RevenueDTO Revenue { get; set; }
    }
}
