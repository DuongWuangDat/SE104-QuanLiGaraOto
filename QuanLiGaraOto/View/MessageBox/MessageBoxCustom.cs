﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLiGaraOto.View.MessageBox
{
    internal class MessageBoxCustom
    {
        public static int Success = 1;
        public static int Error = 2;
        public static void Show(int type, string message)
        {
            if (type == Success)
            {
                // show Success 
                new Success(message).ShowDialog();
            }
            else if (type == Error)
            {
                new Error(message).ShowDialog();
            }
        }
    }
}
