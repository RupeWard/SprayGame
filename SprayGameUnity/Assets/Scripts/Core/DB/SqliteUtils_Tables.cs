using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;

public partial class SqliteUtils : RJWard.Core.Singleton.SingletonApplicationLifetimeLazy<SqliteUtils>
{
	static readonly bool DEBUG_TABLES= true;

	public enum ColumnType
	{
		Text,
		Integer,
		Float,
		Bool,
		Vector2,
		Vector3,
		Colour
	}

	public static readonly Dictionary<ColumnType, string> dbTypeNames = new Dictionary<ColumnType, string>
	{
		{ ColumnType.Text, "TEXT" },
		{ ColumnType.Integer, "INTEGER" },
		{ ColumnType.Float, "TEXT" },
		{ ColumnType.Bool, "INTEGER" },
        { ColumnType.Vector2, "TEXT" },
		{ ColumnType.Vector3, "TEXT" },
		{ ColumnType.Colour, "TEXT" },
	};

	public abstract class Column
	{
		private SqliteParameter param_ = new SqliteParameter();
		public SqliteParameter param
		{
			get { return param_; }
		}

		private string name_;
		public string name
		{
			get { return name_;  }
		}
		private ColumnType colType_;
		public ColumnType colType
		{
			get { return colType_; }
		}

		protected Column( string s, ColumnType c)
		{
			name_ = s;
			colType_ = c;
		}

		public void AddCreateEntry(System.Text.StringBuilder sb)
		{
			sb.Append( name_ ).Append( " " ).Append( dbTypeNames[colType_] );
		}

		public virtual void SetParamValue< T >(T value)
		{
			param.Value = value.ToString( );
		}
	}

	public class TextColumn : Column
	{
		public TextColumn(string n): base( n, ColumnType.Text)
		{
		}

		public string Read(SqliteDataReader reader, int col)
		{
			return reader.GetString( col );
		}
	}

	public class IntegerColumn : Column
	{
		public IntegerColumn( string n ) : base( n, ColumnType.Integer)
		{
		}

		public int Read( SqliteDataReader reader, int col )
		{
			return reader.GetInt32( col );
		}
	}

	public class FloatColumn : Column
	{
		public FloatColumn( string n ) : base( n, ColumnType.Float )
		{
		}

		public float Read( SqliteDataReader reader, int col )
		{
			string s = reader.GetString( col );
			return float.Parse( s );
		}
	}


	public class BoolColumn : Column
	{
		public BoolColumn( string n ) : base( n, ColumnType.Bool )
		{
		}

		public bool Read( SqliteDataReader reader, int col )
		{
			string s = reader.GetString( col );
			return ( s != "0");
		}

		public override void SetParamValue<T>( T value)
		{
			bool? b = value as bool?;
			if (b == null)
			{
				throw new System.InvalidOperationException( "Can't set bool param value to non-bool" );
			}
			else
			{
				param.Value = ((bool)b) ? ("1") : ("0");
			}
		}
	}

	public class Vector2Column : Column
	{
		public Vector2Column( string n ) : base( n, ColumnType.Vector2)
		{
		}

		public Vector2 Read( SqliteDataReader reader, int col )
		{
			Vector2 result = Vector2.zero;
			string s = reader.GetString( col );
            DataHelpers.extractRequiredVector2( ref s, ref result );
			return result;
		}

	}

	public class Vector3Column : Column
	{
		public Vector3Column( string n ) : base( n, ColumnType.Vector2 )
		{
		}

		public Vector3 Read( SqliteDataReader reader, int col )
		{
			Vector3 result = Vector3.zero;
			string s = reader.GetString( col );
			DataHelpers.extractRequiredVector3( ref s, ref result );
			return result;
		}

	}

	public class ColourColumn : Column
	{
		public ColourColumn( string n) : base (n, ColumnType.Colour)
		{ }

		public Color Read (SqliteDataReader reader, int col)
		{
			string s = reader.GetString( col );

			Vector4 v4 = Vector4.zero;

			if (DataHelpers.extractOptionalVector4( ref s, ref v4 ))
			{
				if (v4.x > 255f || v4.x < 0f)
				{
					v4.x = Mathf.Clamp( v4.x, 0f, 255f );
					Debug.LogWarning( "Color.x out of range" );
				}
				if (v4.y > 255f || v4.y < 0f)
				{
					v4.y = Mathf.Clamp( v4.y, 0f, 255f );
					Debug.LogWarning( "Color.y out of range" );
				}
				if (v4.z > 255f || v4.z < 0f)
				{
					v4.z = Mathf.Clamp( v4.z, 0f, 255f );
					Debug.LogWarning( "Color.z out of range" );
				}
				if (v4.w > 255f || v4.w < 0f)
				{
					v4.w = Mathf.Clamp( v4.w, 0f, 255f );
					Debug.LogWarning( "Color.w out of range" );
				}
				v4 = v4 / 255f;
				return new Color( v4.x, v4.y, v4.z, v4.w );
			}
			else
			{
				Vector3 v3 = Vector3.zero;
				s = reader.GetString( col );
				DataHelpers.extractRequiredVector3( ref s, ref v3 );
				if (v3.x > 255f || v3.x < 0f)
				{
					v3.x = Mathf.Clamp( v3.x, 0f, 255f );
					Debug.LogWarning( "Color.x out of range" );
				}
				if (v3.y > 255f || v3.y < 0f)
				{
					v3.y = Mathf.Clamp( v3.y, 0f, 255f );
					Debug.LogWarning( "Color.y out of range" );
				}
				if (v3.z > 255f || v3.z < 0f)
				{
					v3.z = Mathf.Clamp( v3.z, 0f, 255f );
					Debug.LogWarning( "Color.z out of range" );
				}
				v3 = v3 / 255f;
				return new Color( v3.x, v3.y, v3.z );
			}
		}
	}

	public class Table
	{
		private string name_;

		private List<Column> columns_ = new List<Column>();
		
		public Table( string n, List<Column> c )
		{
			name_ = n;
			columns_ = c;
		}

		public string GetSelectCommand()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder( );
			sb.Append( "SELECT " );
			AddAllColsList( sb );
			sb.Append( " FROM " ).Append(name_);
			return sb.ToString( );
		}

		public string GetSelectWhereCommand< T >( Column c, T t)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder( );
			sb.Append( "SELECT " );
			AddAllColsList( sb );
			sb.Append( " FROM " ).Append( name_ );
			sb.Append( " WHERE " ).Append( c.name ).Append( " = '" ).Append( t.ToString( ) ).Append( "'" );
			return sb.ToString( );
		}

		public string GetInsertCommand(bool force)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder( );

			sb.Append("INSERT ");
			if (force)
			{
				sb.Append(" OR REPLACE ");
			}
			else
			{
				sb.Append(" OR IGNORE ");
			}
			sb.Append( " INTO " ).Append( name_ ).Append( " " );
			AddBracketedAllColsList( sb );
			sb.Append( " VALUES " );
			AddPlaceholderList( sb );
			return sb.ToString( );
		}

		private void AddPlaceholderList(System.Text.StringBuilder sb)
		{
			sb.Append( "(" );
			for (int i = 0; i < columns_.Count; i++)
			{
				if (i > 0)
				{
					sb.Append( ", " );
				}
				sb.Append( "?" );
			}
			sb.Append( ")" );
		}

		public void AddParamsToCommand(SqliteCommand command)
		{
			for (int i = 0; i<columns_.Count; i++)
			{
				command.Parameters.Add( columns_[i].param );
            }
		}

		public void AddBracketedAllColsList( System.Text.StringBuilder sb )
		{
			sb.Append( "(" );
			AddAllColsList( sb );
			sb.Append( ")" );
		}

		public void AddAllColsList(System.Text.StringBuilder sb)
		{
			for (int i = 0; i<columns_.Count; i++)
			{
				if (i>0)
				{
					sb.Append( ", " );
				}
				sb.Append( columns_[i].name );
			}
		}

		public bool Create(string dbName, bool bOverwrite)
		{
			if (DEBUG_TABLES)
			{
				Debug.Log( "Craeting table " + name_ + " in " + dbName );
			}
			bool success = false;

			SqliteConnection connection = SqliteUtils.Instance.getConnection( dbName);
			if (connection == null)
			{
				Debug.LogError( "Couldn't get conn to " + dbName + " on creating table " + name_ );
			}
			else
			{
				SqliteCommand createCommand = connection.CreateCommand( );

				System.Text.StringBuilder sb = new System.Text.StringBuilder( );

				sb.Append( "CREATE TABLE " );
				if (!bOverwrite)
				{
					sb.Append( "IF NOT EXISTS " );
				}
				sb.Append( name_ ).Append( " (" );

				for (int i = 0; i < columns_.Count; i++)
				{
					if (i > 0)
					{
						sb.Append( ", " );
					}
					columns_[i].AddCreateEntry( sb );
					if (i == 0)
					{
						sb.Append( " PRIMARY KEY" );
					}
				}
				sb.Append(")");
				createCommand.CommandText = sb.ToString( );

				createCommand.ExecuteNonQuery( );
				createCommand.Dispose( );

				success = true;
			}
			return success;

		}
	}
}
