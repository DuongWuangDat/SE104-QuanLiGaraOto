﻿using QuanLiGaraOto.Model.service;
using QuanLiGaraOto.View.MessageBox;
using QuanLiGaraOto.ViewModel.SuaChuaXeVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuanLiGaraOto.View.SuaChuaXe
{
    /// <summary>
    /// Interaction logic for QuanLiTienCong.xaml
    /// </summary>
    public partial class QuanLiTienCong : Window
    {
        public QuanLiTienCong()
        {
            InitializeComponent();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (DataContext as SuaChuaXeViewModel).DeleteWage.Execute(new object());
        }
    }
}
