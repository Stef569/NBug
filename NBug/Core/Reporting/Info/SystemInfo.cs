// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemInfo.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Security.Principal;

namespace NBug.Core.Reporting.Info
{
	public class SystemInfo
	{

		public static string MachineName
		{
			get { return Environment.MachineName; }
		}

		public static string RuntimeVersion
		{
			get { return Environment.Version.ToString(); }
		}

		#region IP

		public static string FindLocalIPAddress()
		{
			return FindIPAddress(Dns.GetHostEntry(Dns.GetHostName()), true).ToString();
		}

		private static IPAddress FindIPAddress(IPHostEntry host, bool localPreference)
		{
			if (host.AddressList.Length == 1)
				return host.AddressList[0];

			foreach (IPAddress address in host.AddressList)
			{
				bool local = IsLocal(address);

				if (local && localPreference)
					return address;
				else if (!local && !localPreference)
					return address;
			}

			return host.AddressList[0];
		}

		private static bool IsLocal(IPAddress address)
		{
			byte[] addr = address.GetAddressBytes();

			return addr[0] == 10
						 || (addr[0] == 192 && addr[1] == 168)
						 || (addr[0] == 172 && addr[1] >= 16 && addr[1] <= 31);
		}

		#endregion

		#region User

		public static string CurrentUserName
		{
			get { return UserIdentity(); }
		}

		/// <summary>
		/// retrieve identity with fallback on error to safer method
		/// </summary>
		private static string UserIdentity()
		{
			string windowsIdentity = CurrentWindowsIdentity();

			if (string.IsNullOrEmpty(windowsIdentity))
			{
				windowsIdentity = CurrentEnvironmentIdentity();
			}

			return windowsIdentity;
		}

		/// <summary>
		/// exception-safe WindowsIdentity.GetCurrent retrieval returns "domain\username"
		/// per MS, this sometimes randomly fails with "Access Denied" particularly on NT4
		/// </summary>
		private static string CurrentWindowsIdentity()
		{
			try
			{
				return WindowsIdentity.GetCurrent().Name;
			}
			catch (Exception ex)
			{
				return "";
			}
		}

		/// <summary>
		/// exception-safe "domain\username" retrieval from Environment
		/// </summary>
		private static string CurrentEnvironmentIdentity()
		{
			try
			{
				return Environment.UserDomainName + "\\" + Environment.UserName;
			}
			catch (Exception ex)
			{
				return "";
			}
		}

		#endregion
	}
}