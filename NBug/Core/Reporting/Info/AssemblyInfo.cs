// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#region

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

#endregion

namespace NBug.Core.Reporting.Info
{
	public class AssemblyInfo
	{
		private static NameValueCollection assemblyAttributes;

		#region Properties

		public static string AppBuildDate
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["builddate"];
			}
		}

		public static string AppProduct
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["product"];
			}
		}

		public static string AppCompany
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["company"];
			}
		}

		public static string AppCopyright
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["copyright"];
			}
		}

		public static string AppDescription
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["description"];
			}
		}

		public static string AppTitle
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["title"];
			}
		}

		public static string AppFileName
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["filename"];
			}
		}

		public static string AppPath
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["codebase"];
			}
		}

		public static string AppFullName
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["fullname"];
			}
		}

		public static string AppVersion
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["version"];
			}
		}

		public static string CurrentCulture
		{
			get { return CultureInfo.CurrentCulture.ToString(); }
		}

		public static string CallingAssemblyVersion
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["productversion"];
			}
		}

		public static string ExecutingAssemblyName
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["execpath"];
			}
		}

		public static string ExecutingAssemblyPath
		{
			get
			{
				ReadAssemblyAttributesOnce();
				return assemblyAttributes["execfilename"];
			}
		}

		#endregion Properties

		/// <summary>
		/// Reads name-value pair of all assembly attributes into the assemblyAttributes collection.
		/// Note that Assembly values are pulled from the AssemblyInfo file in the project folder.
		/// </summary>
		private static void ReadAssemblyAttributesOnce()
		{
			if (assemblyAttributes != null) return;

			Assembly assembly = GetEntryAssembly();
			NameValueCollection assemblyValues = new NameValueCollection();
			object[] attributes = assembly.GetCustomAttributes(false);

			foreach (object attribute in attributes)
			{
				string attribName = attribute.GetType().ToString();
				string attribValue = "";

				switch (attribName)
				{
				case "System.Reflection.AssemblyTrademarkAttribute":
					attribName = "trademark";
					attribValue = ((AssemblyTrademarkAttribute)attribute).Trademark;
					break;
				case "System.Reflection.AssemblyProductAttribute":
					attribName = "product";
					attribValue = ((AssemblyProductAttribute)attribute).Product;
					break;
				case "System.Reflection.AssemblyCopyrightAttribute":
					attribName = "copyright";
					attribValue = ((AssemblyCopyrightAttribute)attribute).Copyright;
					break;
				case "System.Reflection.AssemblyCompanyAttribute":
					attribName = "company";
					attribValue = ((AssemblyCompanyAttribute)attribute).Company;
					break;
				case "System.Reflection.AssemblyTitleAttribute":
					attribName = "title";
					attribValue = ((AssemblyTitleAttribute)attribute).Title;
					break;
				case "System.Reflection.AssemblyDescriptionAttribute":
					attribName = "description";
					attribValue = ((AssemblyDescriptionAttribute)attribute).Description;
					break;
				}

				if (!string.IsNullOrEmpty(attribValue))
				{
					if (string.IsNullOrEmpty(assemblyValues[attribName]))
					{
						assemblyValues.Add(attribName, attribValue);
					}
				}
			}

			// add some extra values that are not in the AssemblyInfo, but nice to have
			assemblyValues.Add("codebase", assembly.CodeBase.Replace("file:///", ""));
			assemblyValues.Add("builddate", AssemblyBuildDate(assembly).ToString());
			assemblyValues.Add("version", assembly.GetName().Version.ToString());
			assemblyValues.Add("fullname", assembly.FullName);
			assemblyValues.Add("filename", assembly.GetLoadedModules()[0].Name);

			var executingAssembly = Assembly.GetExecutingAssembly();
			assemblyValues.Add("execfilename", executingAssembly.GetName().ToString());

			string execAssemblyPath = Path.GetDirectoryName(new Uri(executingAssembly.CodeBase).LocalPath);
			if (execAssemblyPath != null)
			{
				assemblyValues.Add("execpath", execAssemblyPath);
			}

			var callingAssembly = Assembly.GetCallingAssembly();
			if (callingAssembly.Location != null)
			{
				string productVersion = FileVersionInfo.GetVersionInfo(callingAssembly.Location).ProductVersion;
				assemblyValues.Add("productversion", productVersion);
			}

			assemblyAttributes = assemblyValues;
		}

		private static Assembly GetEntryAssembly()
		{
			return Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
		}

		/// <summary>
		/// Exception-safe file attribute retrieval. We don't care if this fails
		/// </summary>
		private static DateTime AssemblyFileTime(Assembly objAssembly)
		{
			try
			{
				return File.GetLastWriteTime(objAssembly.Location);
			}
			catch (Exception ex)
			{
				return DateTime.MaxValue;
			}
		}

		/// <summary>
		/// Returns build datetime of assembly
		/// assumes default assembly value in AssemblyInfo:
		/// Assembly: AssemblyVersion("1.0.*")
		///
		/// filesystem create time is used, if revision and build were overridden by user
		/// </summary>
		private static DateTime AssemblyBuildDate(Assembly objAssembly, bool forceFileDate = false)
		{
			Version objVersion = objAssembly.GetName().Version;
			DateTime dtBuild;

			if (forceFileDate)
			{
				dtBuild = AssemblyFileTime(objAssembly);
			}
			else
			{
				dtBuild = Convert.ToDateTime("01/01/2000")
								 .AddDays((double)objVersion.Build)
								 .AddSeconds((double)(objVersion.Revision * 2));

				if (TimeZone.IsDaylightSavingTime(DateTime.Now, TimeZone.CurrentTimeZone.GetDaylightChanges(DateTime.Now.Year)))
				{
					dtBuild = dtBuild.AddHours(1);
				}
				if (dtBuild > DateTime.Now | objVersion.Build < 730 | objVersion.Revision == 0)
				{
					dtBuild = AssemblyFileTime(objAssembly);
				}
			}

			return dtBuild;
		}
	}
}