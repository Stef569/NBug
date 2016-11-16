// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TemplateParser.cs" company="NBug Project">
//	 Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Helpers
{
	#region

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Core.Reporting.Info;
	using Core.Util.Serialization;

	#endregion

	/// <summary>
	/// The template parser reads a template and replaces
	/// the following default tokens with their corresponding values.
	/// Tokens start with a '{' and end with a '}' they are always in lower case.
	/// 
	/// Default tokens:
	/// {app.name}
	/// {app.company}
	/// {app.version}
	/// {app.path}
	/// {app.builddate}
	/// {app.culture}
	/// 
	/// {machine.name}
	/// {machine.ip}
	/// {machine.user}
	/// {clr.version}
	/// {nbug.version}
	/// {bug.submission.date}
	/// {bug.submission.usercomment}
	/// 
	/// {exception.date}
	/// {exception.source}
	/// {exception.type}
	/// {exception.message}
	/// {exception.target}
	/// {exception.stacktrace}
	/// {exception.extendedinformation}
	/// 
	/// The token name is not replaced if the value is null.
	/// 
	/// For example if the template contains {app.company} but 
	/// there is no 'company' value. The resulting string returned by the parse method will contain {app.company}
	/// </summary>
	public class TemplateParser
	{
		private readonly SerializableException exception;
		private readonly Report report;
		private readonly Dictionary<string, string> userDefinedTokens;

		/// <summary>
		/// Creates a template parser that can replace application, exception and machine tokens.
		/// The class summary contains the tokens that can be replaced.
		/// </summary>
		public TemplateParser(Report report, SerializableException exception)
		{
			this.exception = exception;
			this.report = report;
			this.userDefinedTokens = new Dictionary<string, string>();
		}

		/// <summary>
		/// Add a user defined token.
		/// This token will be replaced in the Parse method by the given value.
		/// </summary>
		/// <param name="token">The name of the token Like: {bugid}</param>
		/// <param name="value">The token value Like: 5001</param>
		public void AddToken(string token, string value)
		{
			userDefinedTokens.Add(token, value);
		}

		/// <summary>
		/// Replace each token with the corresponding value.
		/// Note: the token name will be displayed if the value for a token is null!
		/// </summary>
		/// <param name="template">The template text with the tokens to be replaced</param>
		/// <returns>The template where each token is replaced with a value</returns>
		public string Parse(string template)
		{
			StringBuilder sb = new StringBuilder(template);
			GeneralInfo generalInfo = report.GeneralInfo;

			// Replace user defined tokens
			foreach (var key in userDefinedTokens.Keys)
			{
				string value = userDefinedTokens[key];
				ReplaceToken(key, value, sb);
			}

			// Replace default tokens
			ReplaceToken("{app.name}", generalInfo.HostApplication, sb);
			ReplaceToken("{app.company}", AssemblyInfo.AppCompany, sb);
			ReplaceToken("{app.version}", generalInfo.HostApplicationVersion, sb);
			ReplaceToken("{app.path}", AssemblyInfo.AppPath, sb);
			ReplaceToken("{app.builddate}", AssemblyInfo.AppBuildDate, sb);
			ReplaceToken("{app.culture}", AssemblyInfo.CurrentCulture, sb);

			ReplaceToken("{machine.name}", SystemInfo.MachineName, sb);
			ReplaceToken("{machine.ip}", SystemInfo.FindLocalIPAddress(), sb);
			ReplaceToken("{machine.user}", SystemInfo.CurrentUserName, sb);

			ReplaceToken("{clr.version}", generalInfo.CLRVersion, sb);
			ReplaceToken("{nbug.version}", generalInfo.NBugVersion, sb);
			ReplaceToken("{exception.date}", generalInfo.DateTime, sb);
			ReplaceToken("{bug.submission.usercomment}", generalInfo.UserDescription, sb);
			ReplaceToken("{bug.submission.date}", DateTime.UtcNow.ToString(), sb);

			if (exception != null)
			{
				ReplaceToken("{exception.source}", exception.Source, sb);
				ReplaceToken("{exception.type}", exception.GetType().FullName, sb);
				ReplaceToken("{exception.message}", exception.Message, sb);
				ReplaceToken("{exception.target}", exception.TargetSite, sb);
				ReplaceToken("{exception.stacktrace}", exception.StackTrace, sb);

				if (exception.ExtendedInformation != null)
				{
					string extendedInfo = string.Join(Environment.NewLine,
					exception.ExtendedInformation.Select(x => x.Key + "=" + x.Value).ToArray());
					ReplaceToken("{exception.extendedinformation}", extendedInfo, sb);
				}
			}

			return sb.ToString();
		}

		private static void ReplaceToken(string token, string value, StringBuilder sb)
		{
			if (value != null)
			{
				sb.Replace(token, value);
			}
		}
	}
}