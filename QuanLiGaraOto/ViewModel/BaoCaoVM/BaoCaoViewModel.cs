﻿using MaterialDesignThemes.Wpf;
using QuanLiGaraOto.DTOs;
using QuanLiGaraOto.Model;
using QuanLiGaraOto.Model.service;
using QuanLiGaraOto.Utils;
using QuanLiGaraOto.View.BaoCao;
using System.Collections.ObjectModel;
using System.Globalization;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using QuanLiGaraOto.View.MessageBox;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Word;

namespace QuanLiGaraOto.ViewModel.BaoCaoVM
{
    internal class BaoCaoViewModel : BaseViewModel
    {
        private int _id;
        private int _month;
        private int _year;
        public List<int> stt;

        private TonKho curTonKho;
        private DoanhThu curDoanhThu;
        public int ID { get { return _id; } set {  _id = value; } }

        public int Month { get { return _month; } set { _month = value; OnPropertyChanged(); } }
        public int Year { get { return _year; } set { _year = value; OnPropertyChanged(); } }

        public List<int> STT { get { return stt; } }

        private ObservableCollection<int> _monthList;
        public ObservableCollection<int> MonthList
        {
            get { return _monthList; }
            set { _monthList = value; OnPropertyChanged(nameof(MonthList)); }
        }
        private ObservableCollection<int> _yearList;
        public ObservableCollection<int> YearList
        {
            get { return _yearList; }
            set { _yearList = value; OnPropertyChanged(nameof(YearList)); }
        }

        // Inventory
        private InventoryReportDTO _currentInventoryReport;
        public InventoryReportDTO CurrentInventoryReport
        {
            get { return _currentInventoryReport; }
            set { _currentInventoryReport = value;}
        }

        private ObservableCollection<InventoryReportDetailDTO> _inventoryDetails;
        public ObservableCollection<InventoryReportDetailDTO> InventoryDetails
        {
            get { return _inventoryDetails; }
            set { _inventoryDetails = value;  }
        }

        private UserControl _currentUserControl;
        public UserControl CurrentUserControl
        {
            get => _currentUserControl;
            set => SetProperty(ref _currentUserControl, value);
        }
        // InventoryCommand---------------------------

        public ICommand GetInventoryReport { get; set; }

        // Revenue-----------------------------
        private RevenueDTO _revenue;
        public RevenueDTO RevenueReport
        {
            get { return _revenue; }
            set { _revenue = value; }
        }

        private ObservableCollection<RevenueDetailDTO> _revenueList;
        public ObservableCollection<RevenueDetailDTO> RevenueList
        {
            get { return _revenueList; }
            set { _revenueList = value; }
        }

        // Revenue Command
        public ICommand GetRevenue { get; set; }

        //---------------------------------------------------
       
        public ICommand FirstLoad { get; set; }
        public ICommand OpenBaoCaoTonKho {  get; set; }
        public ICommand OpenBaoCaoDoanhThu { get; set; }
        public ICommand PrintBaoCao {  get; set; }

        
        public BaoCaoViewModel() { 
            // PageCommand
            FirstLoad = new RelayCommand<object>(_=> true, _=>
            {
                var curDate = DateTime.Now;
                var curYear = curDate.Year;
                var years = Enumerable.Range(2020, (curYear - 2019)).ToList();
                YearList = new ObservableCollection<int>(years);
                var curMonth = curDate.Month;
                var months = Enumerable.Range(1, 12).ToList();
                MonthList = new ObservableCollection<int>(months);
                Year = curYear;
                Month = curMonth;
            });
            OpenBaoCaoTonKho = new RelayCommand<object>(_ => true, (param) =>
            {
                curTonKho = new TonKho();
                CurrentUserControl = curTonKho;

            });
            OpenBaoCaoDoanhThu = new RelayCommand<object>(_ => true, (param) =>
            {
                curDoanhThu = new DoanhThu();
                CurrentUserControl = curDoanhThu;
            });

            PrintBaoCao = new RelayCommand<object>(_=> true, _ =>
            {
                // Kiểm tra xem UserControl hiện tại có phải là BaoCaoDoanhThu không
                if (CurrentUserControl is DoanhThu)
                {
                    //// Chụp ảnh chụp màn hình của UserControl
                    //RenderTargetBitmap rtb = new RenderTargetBitmap((int)curDoanhThu.ActualWidth, (int)curDoanhThu.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                    //rtb.Render(curDoanhThu);

                    //// Lưu hình ảnh vào tệp tạm thời
                    //string tempFilePath = Path.Combine(Path.GetTempPath(), "tempImage.png");
                    //using (var fileStream = new FileStream(tempFilePath, FileMode.Create))
                    //{
                    //    PngBitmapEncoder pngToFile = new PngBitmapEncoder();
                    //    pngToFile.Frames.Add(BitmapFrame.Create(rtb));
                    //    pngToFile.Save(fileStream);
                    //}

                    // Tạo một tài liệu Word mới
                    var wordApp = new Microsoft.Office.Interop.Word.Application();
                    var document = wordApp.Documents.Add();
                    // Thêm header
                    foreach (Microsoft.Office.Interop.Word.Section section in document.Sections)
                    {
                        // Lấy header của mỗi section
                        Microsoft.Office.Interop.Word.HeaderFooter header = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                        header.Range.Text = "Báo cáo doanh thu tháng "+ Month.ToString() +" năm " + Year.ToString();
                        header.Range.Font.Size = 32;
                        header.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    }

                    //// Chèn hình ảnh từ tệp tạm thời vào tài liệu
                    //document.InlineShapes.AddPicture(tempFilePath, LinkToFile: false, SaveWithDocument: true);

                    //// Xóa tệp tạm thời
                    //File.Delete(tempFilePath);

                    // Tạo bảng trong tài liệu Word
                     var table = document.Tables.Add(document.Content, RevenueList.Count + 1, 5); // Số cột cố định là 5

                    // Điền dữ liệu từ RevenueList vào bảng
                    table.Cell(1, 1).Range.Text = "STT";
                    table.Cell(1, 2).Range.Text = "Hiệu xe";
                    table.Cell(1, 3).Range.Text = "Số lượt sửa chữa";
                    table.Cell(1, 4).Range.Text = "Thành tiền";
                    table.Cell(1, 5).Range.Text = "Tỉ lệ";
                    for (int i = 0; i < RevenueList.Count; i++)
                    {
                        var item = RevenueList[i];
                        table.Cell(i + 2, 1).Range.Text = i.ToString();
                        table.Cell(i + 2, 2).Range.Text = item.BrandCar.Name;
                        table.Cell(i + 2, 3).Range.Text = item.RepairCount.ToString();
                        table.Cell(i + 2, 4).Range.Text = item.Price.ToString();
                        table.Cell(i + 2, 5).Range.Text = item.Ratio.ToString();
                    }
                    // Lưu tài liệu
                    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "BaoCaoDoanhThu.docx");
                    document.SaveAs2(filePath);
                    document.Close();

                    // Đóng ứng dụng Word
                    wordApp.Quit();

                    MessageBoxCustom.Show(MessageBoxCustom.Success,"Báo cáo doanh thu đã được in thành công tại: " + filePath);
                }
                else
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Vui lòng mở Báo cáo doanh thu trước khi in.");
                }
            });

            // InventoryCommand

            GetInventoryReport = new RelayCommand<object>(_ => true, async _ =>
            {
                var curDate = DateTime.Now;
                var curMonth = curDate.Month;
                var curYear = curDate.Year;
                if ( Year > curYear)
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Năm lớn hơn hiện tại, vui lòng nhập lại.");
                }
                else if (Month > curMonth && Year == curYear)
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Tháng lớn hơn hiện tại, vui lòng nhập lại.");
                }

                CurrentInventoryReport = await InvetoryReportService.Ins.GetInventoryReport(this.Month, this.Year);
                if (CurrentInventoryReport != null)
                {
                    InventoryDetails = new ObservableCollection<InventoryReportDetailDTO>(CurrentInventoryReport.InventoryReportDetails);
                }
            });

            // Revenue Command

            GetRevenue = new RelayCommand<object>(_ => true, async _ =>
            {
                var curDate = DateTime.Now;
                var curMonth = curDate.Month;
                var curYear = curDate.Year;
                if (Year > curYear)
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Năm lớn hơn hiện tại, vui lòng nhập lại.");
                }
                else if (Month > curMonth && Year == curYear)
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Tháng lớn hơn hiện tại, vui lòng nhập lại.");
                }

                RevenueReport = await RevenueService.Ins.GetRevenue(this.Month, this.Year);
                if (RevenueReport != null)
                {
                    RevenueList = new ObservableCollection<RevenueDetailDTO>(RevenueReport.RevenueDetails);
                    for(int i=0; i<RevenueList.Count; i++)
                    {
                        STT.Add(i);
                    }
                }
            });

        }
        

    }
}
