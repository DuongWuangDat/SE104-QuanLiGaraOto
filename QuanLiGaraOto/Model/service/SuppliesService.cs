﻿using QuanLiGaraOto.DTOs;
using QuanLiGaraOto.View.MessageBox;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiGaraOto.Model.service
{
    public class SuppliesService
    {
        public SuppliesService() { }
        private static SuppliesService _ins;
        public static SuppliesService Ins
        {
            get
            {
                if (_ins == null)
                    _ins = new SuppliesService();
                return _ins;
            }
            private set { _ins = value; }
        }

        public async Task<List<SupplyDTO>> GetAllSupply()
        {
            using(var context = new QuanLiGaraOtoEntities())
            {
                var supplyList = (from s in context.Supplies
                                where s.IsDeleted == false
                                select new SupplyDTO
                                {
                                    ID = s.ID,
                                    Name = s.Name,
                                    CountInStock = s.CountInStock,
                                    InputPrices = s.InputPrices,
                                    OutputPrices = s.OutputPrices,
                                }).ToListAsync();
                return await supplyList;
            }
        }

        public async Task<(bool, string)> AddNewSupply(SupplyDTO newSupply)
        {
            using(var context = new QuanLiGaraOtoEntities())
            {
                var supply = new Supply
                {
                    Name = newSupply.Name,
                    CountInStock = 0,
                    InputPrices = 0,
                    OutputPrices = 0,
                    IsDeleted = false
                };
                //Modify inventory report
                var isSuccess = await InvetoryReportService.Ins.AddNewSupply(supply);
                if (!isSuccess)
                {
                    return (false, "Something went wrong");
                }
                //------------------------
                context.Supplies.Add(supply);
                await context.SaveChangesAsync();
                return (true, "Add new supply successfully!");
            }
        }

        public async Task<(bool, string)> DeleteSupply(int id)
        {
            using(var context = new QuanLiGaraOtoEntities())
            {
                var supply = await context.Supplies.Where(s => s.ID == id).FirstOrDefaultAsync();
                if (supply == null)
                {
                    return (false, "Supply is not exist!");
                }
                supply.IsDeleted = true;
                //Modify inventory report
                var isSuccess = await InvetoryReportService.Ins.DeleteDetail(supply.ID);
                if (!isSuccess)
                {
                    return (false, "Something went wrong");
                }
                //----------------------
                await context.SaveChangesAsync();
                return (true, "Delete supply successfully!");
            }
        }

        public async Task<(bool, string)> UpdateSupply(int id, SupplyDTO newSupply)
        {
            using(var context = new QuanLiGaraOtoEntities())
            {
                var supply = await context.Supplies.Where(s => s.ID == id).FirstOrDefaultAsync();
                if (supply == null)
                {
                    return (false, "Supply is not exist!");
                }
                supply.Name = newSupply.Name;
                supply.CountInStock = newSupply.CountInStock;
                supply.InputPrices = newSupply.InputPrices;
                supply.OutputPrices = newSupply.OutputPrices;
                await context.SaveChangesAsync();
                return (true, "Update supply successfully!");
            }
        }

        public async Task<(bool,string)> AddSupplyInput(SuppliesInputDTO newSuppliesInput)
        {
            using(var context = new QuanLiGaraOtoEntities())
            {
                int? maxID = await context.SuppliesInputs.MaxAsync(b => (int?)b.ID);

                int curID = 0;

                if (maxID.HasValue)
                {
                    curID = (int)maxID + 1;
                }
                else
                {
                    curID = 1;
                }
                var supplyInput = new SuppliesInput
                {
                    DateInput = newSuppliesInput.DateInput,
                    TotalMoney = newSuppliesInput.TotalMoney,
                    IsDeleted = false
                };
                List<SuppliesInputDetail> suppliesInputDetails = new List<SuppliesInputDetail>();
                foreach(var suppliesInputDT in newSuppliesInput.SuppliesInputDetails)
                {
                    var supplyInputDetail = new SuppliesInputDetail
                    {
                        InputID = curID,
                        SuppliesID = suppliesInputDT.Supply.ID,
                        Count = suppliesInputDT.Count,
                        PriceItem = suppliesInputDT.PriceItem,
                        IsDeleted= false
                    };
                    
                    var supply = await context.Supplies.Where(s => s.ID == suppliesInputDT.Supply.ID).FirstOrDefaultAsync();
                    supply.CountInStock += suppliesInputDT.Count;
                    supply.InputPrices = suppliesInputDT.PriceItem;
                    double valuePara = await ParameterService.Ins.GetValueByName("TiLeDonGia");
                    supply.OutputPrices = (decimal)(supply.InputPrices * (decimal)valuePara);
                    await context.SaveChangesAsync();
                    suppliesInputDetails.Add(supplyInputDetail);
                    //Modify inventory report
                    var isSuccessPhatSinh = await InvetoryReportService.Ins.UpdatePhatSinh((int)supplyInputDetail.Count, supplyInputDetail.SuppliesID);
                    var isSuccessTonCuoi = await InvetoryReportService.Ins.UpdateTonCuoi((int)supply.CountInStock, suppliesInputDT.Supply.ID);
                    if (!isSuccessPhatSinh || !isSuccessTonCuoi)
                    {
                        return (false, "Something went wrong");
                    }
                    //-----------------------
                }
                context.SuppliesInputDetails.AddRange(suppliesInputDetails);
                context.SuppliesInputs.Add(supplyInput);
                await context.SaveChangesAsync();
                return (true, "Add supply input successfully!");
            }
        }
        public async Task<int> CountSupply()
        {
            using (var context = new QuanLiGaraOtoEntities())
            {
                var count = await context.Supplies.CountAsync();
                return count;
            }
        }
    }
}