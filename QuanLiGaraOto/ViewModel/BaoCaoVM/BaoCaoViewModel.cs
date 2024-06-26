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
using System.Threading;

namespace QuanLiGaraOto.ViewModel.BaoCaoVM
{
    internal class BaoCaoViewModel : BaseViewModel
    {
        private int _id;
        private int _month;
        private int _year;

        private TonKho curTonKho;
        private DoanhThu curDoanhThu;
        public int ID { get { return _id; } set {  _id = value; } }

        public int Month { get { return _month; } set { _month = value; OnPropertyChanged(); } }
        public int Year { get { return _year; } set { _year = value; OnPropertyChanged(); } }


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
            set { _currentInventoryReport = value; }
        }

        private ObservableCollection<InventoryReportDetailDTO> _inventoryDetails;
        public ObservableCollection<InventoryReportDetailDTO> InventoryDetails
        {
            get { return _inventoryDetails; }
            set { _inventoryDetails = value;  OnPropertyChanged(); }
        }

        private UserControl _currentUserControl;
        public UserControl CurrentUserControl
        {
            get => _currentUserControl;
            set => SetProperty(ref _currentUserControl, value);
        }

        private Visibility _isNullVisible;
        public Visibility IsNullVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; OnPropertyChanged(); }
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
            set { _revenueList = value; OnPropertyChanged(); }
        }

        private decimal _totalprice;
        public decimal TotalPrice { 
            get { return _totalprice; } 
            set { _totalprice = value; OnPropertyChanged(); }
        }

        private Visibility _isVisible;
        public Visibility IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; OnPropertyChanged(); }
        }

        // Revenue Command
        public ICommand GetRevenue { get; set; }

        //---------------------------------------------------
       
        public ICommand FirstLoad { get; set; }
        public ICommand OpenBaoCaoTonKho {  get; set; }
        public ICommand OpenBaoCaoDoanhThu { get; set; }
        public ICommand PrintBaoCao {  get; set; }

        
        public BaoCaoViewModel() { 
            IsVisible = Visibility.Hidden;
            IsNullVisible = Visibility.Hidden;
            var curTime = DateTime.Now;
            Year = curTime.Year;
            Month = (curTime.Month - 1);
            // PageCommand
            FirstLoad = new RelayCommand<object>(_=> true, _=>
            {
                IsVisible = Visibility.Hidden;
                IsNullVisible = Visibility.Hidden;
                if(CurrentUserControl is DoanhThu)
                {
                    IsVisible = Visibility.Visible;
                }
                var curDate = DateTime.Now;
                var curYear = curDate.Year;
                var years = Enumerable.Range(2020, (curYear - 2019)).ToList();
                YearList = new ObservableCollection<int>(years);
                var curMonth = (curDate.Month-1);
                var months = Enumerable.Range(1, 12).ToList();
                MonthList = new ObservableCollection<int>(months);
            });
            OpenBaoCaoTonKho = new RelayCommand<object>(_ => true, async (param) =>
            {
                IsVisible = Visibility.Hidden;
                IsNullVisible = Visibility.Hidden;
                GetInventoryReport.Execute(null);
            });
            OpenBaoCaoDoanhThu = new RelayCommand<object>(_ => true,async (param) =>
            {
                
                IsVisible = Visibility.Hidden;
                IsNullVisible = Visibility.Hidden;
                GetRevenue.Execute(null);
            });

            PrintBaoCao = new RelayCommand<object>(_=> true, _ =>
            {
                var thread = new Thread(() => {
                    // Kiểm tra xem UserControl hiện tại có phải là BaoCaoDoanhThu không
                    if (CurrentUserControl is DoanhThu)
                    {

                        // Tạo một tài liệu Word mới
                        var wordApp = new Microsoft.Office.Interop.Word.Application();
                        var document = wordApp.Documents.Add();
                        // Thêm header
                        foreach (Microsoft.Office.Interop.Word.Section section in document.Sections)
                        {
                            // Lấy header của mỗi section
                            Microsoft.Office.Interop.Word.HeaderFooter header = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                            header.Range.Text = "Báo cáo doanh thu tháng " + Month.ToString() + " năm " + Year.ToString();
                            header.Range.Font.Size = 32;
                            header.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        }


                        // Tạo bảng trong tài liệu Word
                        var table = document.Tables.Add(document.Content, RevenueList.Count + 1, 5); // Số cột cố định là 5
                        table.Borders.Enable = 1;

                        // Điền dữ liệu từ RevenueList vào bảng
                        table.Cell(1, 1).Range.Text = "STT";
                        table.Cell(1, 2).Range.Text = "Hiệu xe";
                        table.Cell(1, 3).Range.Text = "Số lượt sửa chữa";
                        table.Cell(1, 4).Range.Text = "Thành tiền";
                        table.Cell(1, 5).Range.Text = "Tỉ lệ";
                        for (int i = 0; i < RevenueList.Count; i++)
                        {
                            var item = RevenueList[i];
                            table.Cell(i + 2, 1).Range.Text = item.STT.ToString();
                            table.Cell(i + 2, 2).Range.Text = item.BrandCar.Name;
                            table.Cell(i + 2, 3).Range.Text = item.RepairCount.ToString();
                            table.Cell(i + 2, 4).Range.Text = string.Format("{0:N0} VNĐ", item.Price);
                            table.Cell(i + 2, 5).Range.Text = ((double)item.Ratio).ToString("F2");
                        }
                        Microsoft.Office.Interop.Word.Paragraph totalRevenueParagraph = document.Content.Paragraphs.Add();
                        totalRevenueParagraph.Range.Text = "Tổng doanh thu: " + string.Format("{0:N0} VNĐ", TotalPrice);
                        totalRevenueParagraph.Range.InsertParagraphAfter();
                        // Lưu tài liệu
                        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "BaoCaoDoanhThu.docx");
                        document.SaveAs2(filePath);
                        document.Close();

                        // Đóng ứng dụng Word
                        wordApp.Quit();

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxCustom.Show(MessageBoxCustom.Success, "Báo cáo doanh thu đã được tạo thành công tại: " + filePath);
                        });
                    }
                    // Kiểm tra xem UserControl hiện tại có phải là TonKho không
                    else if (CurrentUserControl is TonKho)
                    {
                        // Tạo một tài liệu Word mới
                        var wordApp = new Microsoft.Office.Interop.Word.Application();
                        var document = wordApp.Documents.Add();
                        // Thêm header
                        foreach (Microsoft.Office.Interop.Word.Section section in document.Sections)
                        {
                            // Lấy header của mỗi section
                            Microsoft.Office.Interop.Word.HeaderFooter header = section.Headers[Microsoft.Office.Interop.Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                            header.Range.Text = "Báo cáo tồn kho tháng " + Month.ToString() + " năm " + Year.ToString();
                            header.Range.Font.Size = 32;
                            header.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        }

                        // Tạo bảng trong tài liệu Word
                        var table = document.Tables.Add(document.Content, InventoryDetails.Count + 1, 4); // Số cột cố định là 4
                        table.Borders.Enable = 1;

                        // Điền dữ liệu từ InventoryDetails vào bảng
                        table.Cell(1, 1).Range.Text = "Vật tư phụ tùng";
                        table.Cell(1, 2).Range.Text = "Tồn đầu";
                        table.Cell(1, 3).Range.Text = "Phát sinh";
                        table.Cell(1, 4).Range.Text = "Tồn cuối";
                        for (int i = 0; i < InventoryDetails.Count; i++)
                        {
                            var item = InventoryDetails[i];
                            table.Cell(i + 2, 1).Range.Text = item.SupplyName;
                            table.Cell(i + 2, 2).Range.Text = item.TonDau.ToString();
                            table.Cell(i + 2, 3).Range.Text = item.PhatSinh.ToString();
                            table.Cell(i + 2, 4).Range.Text = item.TonCuoi.ToString();
                        }

                        // Lưu tài liệu
                        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "BaoCaoTonKho.docx");
                        document.SaveAs2(filePath);
                        document.Close();

                        // Đóng ứng dụng Word
                        wordApp.Quit();

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxCustom.Show(MessageBoxCustom.Success, "Báo cáo tồn kho đã được tạo thành công tại: " + filePath);
                        });
                    }
                    else
                    {
                        MessageBoxCustom.Show(MessageBoxCustom.Error, "Vui lòng mở báo cáo trước khi in.");
                    }
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            });

            // InventoryCommand

            GetInventoryReport = new RelayCommand<object>(_ => true, async _ =>
            {
                var curDate = DateTime.Now;
                var curMonth = curDate.Month;
                var curYear = curDate.Year;
                CurrentUserControl = new UserControl();
                if (Month > (curMonth - 1) && Year == curYear)
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Chỉ có thể xem dữ liệu các tháng trước, vui lòng nhập lại.");
                    return ;
                }

                CurrentInventoryReport = await InvetoryReportService.Ins.GetInventoryReport(this.Month, this.Year);
                if (CurrentInventoryReport != null && CurrentInventoryReport.InventoryReportDetails.Count != 0)
                {
                    IsNullVisible = Visibility.Hidden;
                    InventoryDetails = new ObservableCollection<InventoryReportDetailDTO>(CurrentInventoryReport.InventoryReportDetails);
                    curTonKho = new TonKho();
                    CurrentUserControl = curTonKho;
                }
                if(CurrentInventoryReport == null || CurrentInventoryReport.InventoryReportDetails.Count == 0) { IsNullVisible = Visibility.Visible; IsVisible = Visibility.Hidden; }
            });

            // Revenue Command

            GetRevenue = new RelayCommand<object>(_ => true, async _ =>
            {
                var curDate = DateTime.Now;
                var curMonth = curDate.Month;
                var curYear = curDate.Year;
                CurrentUserControl = new UserControl();
                if (Month > (curMonth - 1) && Year == curYear)
                {
                    MessageBoxCustom.Show(MessageBoxCustom.Error, "Chỉ có thể xem dữ liệu các tháng trước, vui lòng nhập lại.");
                    return;
                }

                RevenueReport = await RevenueService.Ins.GetRevenue(this.Month, this.Year);
                if (RevenueReport != null && RevenueReport.RevenueDetails.Count != 0)
                {
                    IsNullVisible = Visibility.Hidden;
                    IsVisible = Visibility.Visible;
                    TotalPrice = (decimal)RevenueReport.TotalPrice;
                    RevenueList = new ObservableCollection<RevenueDetailDTO>(RevenueReport.RevenueDetails);
                    for (int i = 0; i < RevenueList.Count; i++)
                    {
                        RevenueList[i].STT = i + 1;
                    }
                    curDoanhThu = new DoanhThu();
                    CurrentUserControl = curDoanhThu;
                } 
                if(RevenueReport == null || RevenueReport.RevenueDetails.Count == 0) { IsNullVisible = Visibility.Visible; IsVisible = Visibility.Hidden; }
            });

        }
        

    }
}
