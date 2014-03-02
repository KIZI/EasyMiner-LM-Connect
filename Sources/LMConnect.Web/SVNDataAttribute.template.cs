using System;
using System.Reflection;

namespace LMConnect.Web
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class SVNDataAttribute : System.Attribute
	{
		private static SVNDataAttribute _assemblySvnData;

		public static SVNDataAttribute AssemblySVNData
		{
			get
			{
				if (_assemblySvnData == null)
				{
					object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(SVNDataAttribute), false);

					if (attributes.Length > 0)
					{
						_assemblySvnData = attributes[0] as SVNDataAttribute;
					}

					if (_assemblySvnData == null)
					{
						throw new Exception("Assembly should have exactly one SVN Data attribute");
					}
				}

				return _assemblySvnData;
			}
		}

		/*
		WCREV = $WCREV$
		WCDATE = $WCDATE$
		WCNOW = $WCNOW$
		*/

		public string WCREV
		{
			get { return "$WCREV$"; }
		}

		public string WCRANGE
		{
			get { return WCREV; }
		}

		public string WCDATE
		{
			get { return "$WCDATE$"; }
		}

		public string WCNOW
		{
			get { return "$WCNOW$"; }
		}

		public SVNDataAttribute()
		{
		}
	}
}
