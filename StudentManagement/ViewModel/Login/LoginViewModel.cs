﻿using StudentManagement.Model;
using StudentManagement.ViewModel.MessageBox;
using StudentManagement.Views.GiamHieu;
using StudentManagement.Views.GiaoVien;
using StudentManagement.Views.HocSinh;
using StudentManagement.Views.Login;
using StudentManagement.Views.MessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StudentManagement.ViewModel.Login
{
    public class LoginViewModel : BaseViewModel, IDataErrorInfo
    {

        // khai báo biến
        public bool IsLoggedIn { get; set; }
        private string _username;
        public string Username { get => _username; set { _username = value; OnPropertyChanged(); } }
        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }
        private int _indexRole = -1;
        public int IndexRole { get => _indexRole; set { _indexRole = value; OnPropertyChanged(); } }

        // khai báo usercontrol
        public LoginWindow LoginWindow { get; set; }

        // khai báo command
        public ICommand GoToRegisterCommand { get; set; }
        public ICommand GoToForgotPasswordCommand { get; set; }
        public ICommand LoginSuccess { get; set; }
        public ICommand PasswordChangedCommand { get; set; }
        public ICommand PasswordEyeChangedCommand { get; set; }
        public ICommand ShowPassword { get; set; }
        public ICommand UnshowPassword { get; set; }
        public ICommand LoadData { get; set; }
        public ICommand TurnBackRoleForm { get; set; }
        public ICommand TurnToLoginForm { get; set; }
        public ICommand LogOut { get; set; }

        // in lỗi để username trống
        public string Error { get { return null; } }

        public string this[string columnName]
        {
            get
            {
                if (LoginWindow != null)
                {
                    string ErrorMess = null;
                    if (LoginWindow.Account.IsFocused == true)
                    {
                        switch (columnName)
                        {
                            case "Username":
                                if (String.IsNullOrEmpty(Username))
                                    ErrorMess = "Vui lòng nhập username.";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        ErrorMess = "";
                    }
                    return ErrorMess;
                }
                else return null;

            }
        }


        public LoginViewModel()
        {
            IsLoggedIn = false;
            Username = "";
            Password = "";

            // binding text 
            PasswordChangedCommand = new RelayCommand<PasswordBox>((paramater) => { return true; }, (paramater) => { Password = paramater.Password; });
            PasswordEyeChangedCommand = new RelayCommand<TextBox>((paramater) => { return true; }, (paramater) => { Password = paramater.Text; });

            LoadData = new RelayCommand<LoginWindow>((parameter) => { return true; }, (parameter) =>
            {
                LoginWindow = parameter;
                
                
            });

            // navigate
            LoginSuccess = new RelayCommand<Window>((paramater) => { return true; }, (paramater) =>
            {

                if (paramater == null)
                    return;
                if (!ValidateInfo(Username, Password))
                {
                    MessageBoxOK MB = new MessageBoxOK();
                    var data = MB.DataContext as MessageBoxOKViewModel;
                    data.Content = "Vui lòng nhập đầy đủ thông tin";
                    MB.ShowDialog();
                    data.Content = "";
                }
                else
                {
                    if (!CheckInvalidRole(IndexRole))
                    {
                        MessageBoxOK MB = new MessageBoxOK();
                        var data = MB.DataContext as MessageBoxOKViewModel;
                        data.Content = "Vui lòng chọn chức vụ";
                        MB.ShowDialog();
                        return;
                    }
                    string passEncode = CreateMD5(Base64Encode(Password));
                    int id = -1;
                    int checkUser = 0;
                    string CmdString = string.Empty;
                    using (SqlConnection con = new SqlConnection(ConnectionString.connectionString))
                    {
                        try
                        {
                            try
                            {
                                con.Open();
                            }
                            catch (Exception)
                            {
                                MessageBoxFail messageBoxFail = new MessageBoxFail();
                                messageBoxFail.ShowDialog();
                            }
                            CmdString = "Select count(*) from " + (IndexRole == 0 ? "GiamHieu" : IndexRole == 1 ? "GiaoVien" : "HocSinh") + " where Username = '" + Username + "' and UserPassword = '" + passEncode + "'";
                            SqlCommand cmd = new SqlCommand(CmdString, con);
                            checkUser = Convert.ToInt32(cmd.ExecuteScalar());


                            CmdString = "Select " + (IndexRole == 0 ? "MaTruong" : IndexRole == 1 ? "MaGiaoVien" : "MaHocSinh") + " from " + (IndexRole == 0 ? "GiamHieu" : IndexRole == 1 ? "GiaoVien" : "HocSinh") + " where Username = '" + Username + "' and UserPassword = '" + passEncode + "'";
                            cmd = new SqlCommand(CmdString, con);
                            id = Convert.ToInt32(cmd.ExecuteScalar());
                            con.Close();
                        }
                        catch (Exception)
                        {
                        }

                    }
                    if (checkUser > 0)
                    {
                        IsLoggedIn = true;
                        LoginWindow.LoadBorder.Visibility = Visibility.Visible;
                        LoginWindow.Hide();
                        if (IndexRole == 0)
                        {
                            GiamHieuWindow window = new GiamHieuWindow();
                            StudentManagement.ViewModel.GiamHieu.TrangChuViewModel vm = window.DataContext as StudentManagement.ViewModel.GiamHieu.TrangChuViewModel;
                            vm.IdGiamHieu = id;
                            window.ShowDialog();
                            return;
                        }
                        else if (IndexRole == 1)
                        {
                            GiaoVienWindow window = new GiaoVienWindow();
                            StudentManagement.ViewModel.GiaoVien.TrangChuViewModel vm = window.DataContext as StudentManagement.ViewModel.GiaoVien.TrangChuViewModel;
                            vm.CurrentUser.MaGiaoVien = id;
                            window.ShowDialog();
                            return;
                        }
                        else
                        {
                            HocSinhWindow window = new HocSinhWindow();
                            StudentManagement.ViewModel.HocSinh.TrangChuViewModel vm = window.DataContext as StudentManagement.ViewModel.HocSinh.TrangChuViewModel;
                            vm.IdHocSinh = id;
                            window.ShowDialog();
                            return;
                        }
                    }
                    else
                    {
                        IsLoggedIn = false;
                        MessageBoxOK MB = new MessageBoxOK();
                        var data = MB.DataContext as MessageBoxOKViewModel;
                        data.Content = "Sai tài khoản hoặc mật khẩu";
                        MB.ShowDialog();
                        data.Content = "";
                        return;
                    }
                }
            });
            GoToForgotPasswordCommand = new RelayCommand<LoginWindow>((paramater) => { return true; }, (paramater) =>
            {
                Username = "";
                paramater.Hide();
                ForgotPasswordWindow forgotPassword = new ForgotPasswordWindow();
                ForgotPasswordViewModel viewmodel = forgotPassword.DataContext as ForgotPasswordViewModel;
                viewmodel.IndexRole = IndexRole;
                forgotPassword.ShowDialog();
                paramater.Close();

            });

            // show password
            ShowPassword = new RelayCommand<LoginWindow>((paramater) => { return true; }, (paramater) =>
            {
                paramater.ShowPass.Visibility = Visibility.Hidden;
                paramater.UnshowPass.Visibility = Visibility.Visible;
                paramater.PasswordEye.Text = paramater.Password.Password;
                paramater.PasswordEye.Visibility = Visibility.Visible;
                paramater.Password.Visibility = Visibility.Hidden;
            });
            UnshowPassword = new RelayCommand<LoginWindow>((paramater) => { return true; }, (paramater) =>
            {
                paramater.ShowPass.Visibility = Visibility.Visible;
                paramater.UnshowPass.Visibility = Visibility.Hidden;
                paramater.Password.Visibility = Visibility.Visible;
                paramater.Password.Password = paramater.PasswordEye.Text;
                paramater.PasswordEye.Visibility = Visibility.Hidden;
            });

            // switch role and login
            TurnBackRoleForm = new RelayCommand<object>((paramater) => { return true; }, (parameter) =>
            {
                IndexRole = -1;
                LoginWindow.GiamHieuRole.IsChecked = false;
                LoginWindow.GiaoVienRole.IsChecked = false;
                LoginWindow.HocSinhRole.IsChecked = false;
                LoginWindow.LoginForm.Visibility = Visibility.Collapsed;
                LoginWindow.RoleForm.Visibility = Visibility.Visible;
                Username = "";
                LoginWindow.Password.Password = null;

            });
            TurnToLoginForm = new RelayCommand<object>((paramater) => { return true; }, (parameter) =>
            {
                LoginWindow.RoleForm.Visibility = Visibility.Collapsed;
                LoginWindow.LoginForm.Visibility = Visibility.Visible;
                if (LoginWindow.GiaoVienRole.IsChecked == true)
                {
                    IndexRole = 1;
                }
                if (LoginWindow.GiamHieuRole.IsChecked == true)
                {
                    IndexRole = 0;
                }
                if (LoginWindow.HocSinhRole.IsChecked == true)
                {
                    IndexRole = 2;
                }

            });
            LogOut = new RelayCommand<Window>((paramater) => { return true; }, (parameter) =>
            {
                parameter.Close();
                LoginWindow loginWindow = new LoginWindow();
                LoginViewModel vm = loginWindow.DataContext as LoginViewModel;
                vm.Username = "";
                loginWindow.ShowDialog();
            });
        }

        public bool ValidateInfo(string username, string password)
        {
            if ( username == "" || password == "" || username == null || password == null)
                return false;
            return true;
        }

        public bool CheckInvalidRole(int index)
        {
            return index >= 0 && index <= 2;
        }

        // hàm mã hóa mật khẩu
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                //return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
