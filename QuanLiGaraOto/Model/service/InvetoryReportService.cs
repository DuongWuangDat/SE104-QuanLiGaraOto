﻿using MaterialDesignThemes.Wpf.Converters;
using QuanLiGaraOto.DTOs;
using QuanLiGaraOto.View.MessageBox;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiGaraOto.Model.service
{
    public class InvetoryReportService
    {
        public InvetoryReportService() { }
        private static InvetoryReportService _ins;
        public static InvetoryReportService Ins
        {
            get
            {
                if (_ins == null)
                    _ins = new InvetoryReportService();
                return _ins;
            }
            private set { _ins = value; }
        }
        
        
        //Should check current date before get inventory report
        public async Task<InventoryReportDTO> GetInventoryReport(int Month, int Year)
        {
            try
            {
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var inventoryReportList = (from s in context.InventoryReports
                                               where s.IsDeleted == false && s.Month == Month && s.Year == Year
                                               select new InventoryReportDTO
                                               {
                                                   ID = s.ID,
                                                   Month = s.Month,
                                                   Year = s.Year,
                                                   InventoryReportDetails = (from d in s.InventoryReportDetails
                                                                             select new InventoryReportDetailDTO
                                                                             {
                                                                                 TonDau = d.TonDau,
                                                                                 PhatSinh = d.PhatSinh,
                                                                                 TonCuoi = d.TonCuoi,
                                                                                 SupplyID = d.SuppliesID,
                                                                                 SupplyName = d.Supply.Name
                                                                             }).ToList()

                                               }).FirstOrDefaultAsync();
                    return await inventoryReportList;
                }
            }catch(Exception e)
            {
                MessageBoxCustom.Show(MessageBoxCustom.Error, "Không thể lấy dữ liệu");
                return null;
            }
            
        }

        public async Task<(bool, string)> InitInventoryReport()
        {
            try
            {
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var supplies = await SuppliesService.Ins.GetAllSupply();
                    var curDate = DateTime.Now;
                    var curMonth = curDate.Month;
                    var curYear = curDate.Year;
                    var inventoryReport = await GetInventoryReport(curMonth, curYear);
                    if (inventoryReport != null)
                    {
                        return (false, "Báo cáo tồn kho tháng này đã được khởi tạo");
                    }
                    int? maxID = await context.Repairs.MaxAsync(b => (int?)b.ID);

                    int curID = 0;

                    if (maxID.HasValue)
                    {
                        curID = (int)maxID + 1;
                    }
                    else
                    {
                        curID = 1;
                    }
                    var newInventoryReport = new InventoryReport
                    {
                        Month = curMonth,
                        Year = curYear,
                        IsDeleted = false
                    };
                    List<InventoryReportDetail> detailList = new List<InventoryReportDetail>();
                    foreach (var sp in supplies)
                    {
                        var inventoryReportDT = new InventoryReportDetail
                        {
                            InventoryReportID = curID,
                            SuppliesID = sp.ID,
                            TonDau = sp.CountInStock,
                            PhatSinh = 0,
                            TonCuoi = sp.CountInStock,
                            IsDeleted = false
                        };
                        detailList.Add(inventoryReportDT);
                    }
                    context.InventoryReportDetails.AddRange(detailList);
                    context.InventoryReports.Add(newInventoryReport);
                    await context.SaveChangesAsync();
                    return (true, "Initialize inventory report successfully");

                }
            }catch(Exception e)
            {
                return (false, "Some thing went wrong");
            }
            
        }

        public async Task<InventoryReportDTO> GetCurrentInventoryReport()
        {   
            var curDate = DateTime.Now;
            var inventoryReport = await GetInventoryReport(curDate.Month, curDate.Year);
            return inventoryReport;
        }

        public async Task<bool> AddNewSupply(Supply newSupply)
        {
            try
            {
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var inventoryReport = await GetCurrentInventoryReport();
                    var newDetail = new InventoryReportDetail
                    {
                        InventoryReportID = inventoryReport.ID,
                        SuppliesID = newSupply.ID,
                        TonDau = 0,
                        PhatSinh = 0,
                        TonCuoi = 0,
                        IsDeleted = false
                    };
                    context.InventoryReportDetails.Add(newDetail);
                    await context.SaveChangesAsync();
                    return true;
                }
            }catch(Exception e)
            {
                return false;
            }
            
        }

        public async Task<bool> UpdatePhatSinh(int delta, int supplyId)
        {
            try
            {
                if (delta <= 0)
                {
                    return false;
                }
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var inventoryReport = await GetCurrentInventoryReport();
                    var detail = await context.InventoryReportDetails.Where(b => b.SuppliesID == supplyId && b.InventoryReportID == inventoryReport.ID).FirstOrDefaultAsync();
                    detail.PhatSinh += delta;
                    await context.SaveChangesAsync();
                    return true;
                }
            }catch(Exception e)
            {
                return false;
            }
            
        }

        public async Task<bool> UpdateTonCuoi(int tonCuoi, int supplyId)
        {
            try
            {
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var inventoryReport = await GetCurrentInventoryReport();
                    var detail = await context.InventoryReportDetails.Where(b => b.SuppliesID == supplyId && b.InventoryReportID == inventoryReport.ID).FirstOrDefaultAsync();
                    detail.TonCuoi = tonCuoi;
                    await context.SaveChangesAsync();
                    return true;
                }
            }catch(Exception e)
            {
                return false;
            }
            
        }

        public async Task<bool> DeleteDetail(int supplyId)
        {
            try
            {
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var inventoryReport = await GetCurrentInventoryReport();
                    var detail = await context.InventoryReportDetails.Where(b => b.SuppliesID == supplyId && b.InventoryReportID == inventoryReport.ID).FirstOrDefaultAsync();
                    if(detail == null)
                    {
                        return true;
                    }
                    context.InventoryReportDetails.Remove(detail);
                    await context.SaveChangesAsync();
                    return true;
                }
            }
            catch
            {
                return false;
            }
            
        }

        public async Task<bool> RecoveryInventory(Repair repair)
        {
            try
            {
                using (var context = new QuanLiGaraOtoEntities())
                {
                    var inventoryReport = await GetCurrentInventoryReport();
                    foreach (var repairDT in repair.RepairDetails)
                    {
                        foreach (var rpSDT in repairDT.RepairSuppliesDetails)
                        {
                            var detail = await context.InventoryReportDetails.Where(b => b.SuppliesID == rpSDT.SuppliesID && b.InventoryReportID == inventoryReport.ID).FirstOrDefaultAsync();
                            detail.TonCuoi += rpSDT.Count;
                        }
                    }

                    await context.SaveChangesAsync();
                    return true;
                }
            }catch(Exception e)
            {
                return false;
            }
            
        }
    }
}
