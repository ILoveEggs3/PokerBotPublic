using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Shared.Poker.Views
{
    
    /// <summary>
    /// Event handler for the 'CellClicked' event in the poker hand chart.
    /// </summary>
    /// <param name="sender">The chart that was clicked</param>
    /// <param name="hit_cell">The GridCell object representing the clicked cell.</param>
    public delegate void HandClicked(object sender, GridCell hit_cell);

    /// <summary>
    /// Control to display a 13x13 grid, 2 card, poker hand chart.
    /// Each grid cell has a string value and foreground/background colour associated with it.
    /// </summary>
    public partial class PokerHandChart : UserControl, IEnumerable<GridCell>
    {

        private static readonly int CELL_N = 13;

        public static Brush CELL_BG_DEFAULT = Brushes.WhiteSmoke;

        private Font hdr_font, cell_font;
        private Brush hdr_brush = Brushes.Black;
        private GridCell[][] cells;
        private int cell_size = 20;
        private float hdr_font_size = 0.01f, cell_font_size = 0.01f;
        private string hdr_font_name = "Verdana", cell_font_name = "Arial";

        private GridCell[] hdr_row = new GridCell[13], hdr_col = new GridCell[13];

        /// <summary>
        /// Construct a new PokerHandChart object.
        /// </summary>
        public PokerHandChart()
        {
            InitializeComponent();

            cell_font = new Font(cell_font_name, cell_font_size);
            hdr_font = new Font(hdr_font_name, hdr_font_size, FontStyle.Bold);
            // Grid is a 13x13 2D array
            cells = new GridCell[CELL_N][];
            for (int i = 0; i < CELL_N; ++i)
            {
                cells[i] = new GridCell[CELL_N];
                for (int j = 0; j < CELL_N; ++j)
                {
                    cells[i][j] = new GridCell();
                    cells[i][j].ForHand = Hands.handFor(i, j);
                }
            }

            for (int i = 0; i < CELL_N; ++i)
            {
                hdr_row[i] = new GridCell();
                hdr_row[i].DisplayValue = "" + Hands.ranks[i];
                hdr_row[i].Background = Brushes.WhiteSmoke;
                hdr_col[i] = new GridCell();
                hdr_col[i].DisplayValue = "" + Hands.ranks[i];
                hdr_col[i].Background = Brushes.WhiteSmoke;
            }
        }

        /// <summary>
        /// The Brush used to render the 'header' cells, ie the Card
        /// ranks listed at the top and left of the grid.
        /// </summary>
        public Brush HeaderBrush
        {
            get { return hdr_brush; }
            set { hdr_brush = value; }
        }

        /// <summary>
        /// Accessor for font name used to render the data cells.
        /// </summary>
        public String CellFont
        {
            get { return cell_font.Name; }
            set { cell_font = new Font(cell_font_name = value, cell_size); }
        }

        /// <summary>
        /// Accessor for font name used to render 'Header' cells, ie the Card
        /// ranks listed at the top and left of the grid. This font is also
        /// set to Bold.
        /// </summary>
        public String HeaderFont
        {
            get { return hdr_font.Name; }
            set { hdr_font = new Font(hdr_font_name = value, cell_size, FontStyle.Bold); }
        }

        /// <summary>
        /// Fired when a cell is clicked on.
        /// </summary>
        public event HandClicked HandClickedEvent;

        #region Cell Accessors (indexer syntax)
        /// <summary>
        /// Grid Cell access by row and column numer directly, (0,0) = "AA" etc.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public GridCell this[int c1, int c2]
        {
            get
            {
                return cells[c1][c2];
            }
        }

        /// <summary>
        /// GridCell access by ranks, ie this['A', 'K', true] for AKs
        /// Note: case and card order is auto-corrected, thus:
        /// chart['K', '4', true] is exactly equivalent to: chart['4', 'k', true].
        /// The bool parameter is ignored for paired hands.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public GridCell this[char c1, char c2, bool suited]
        {
            get
            {
                c1 = Char.ToUpper(c1); c2 = Char.ToUpper(c2);
                // Ensure cannonical ordering of cards -> Big-Small
                if (Hands.indexFor(c1) > Hands.indexFor(c2))
                {
                    char tmp = c1;
                    c1 = c2;
                    c2 = tmp;
                }
                if (c1 == c2)
                    return this[string.Format("{0}{1}", c1, c2)];
                else
                    return this[string.Format("{0}{1}{2}", c1, c2, suited ? "s" : "o")];
            }
        }

        /// <summary>
        /// Grid Cell access by hand, ie this["AKs"]
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public GridCell this[string hand]
        {
            get
            {
                int r, c;
                Hands.RowColFor(hand, out r, out c);
                return cells[r][c];
            }
        }
        #endregion

        /// <summary>
        /// User drawn Control - graphical interface drawn here....
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Pen p = new System.Drawing.Pen(Color.Black);
            
            /// Draw Header Row and column
            GridCell c;
            StringFormat fmt = new StringFormat();
            fmt.Alignment = StringAlignment.Center;
            fmt.LineAlignment = StringAlignment.Center;
            for (int i = 0; i < CELL_N; ++i)
            {
                c = hdr_row[i];
                g.FillRectangle(c.Background, c.Bounds);
                g.DrawRectangle(p, c.Bounds);
                g.DrawString(c.DisplayValue, hdr_font, hdr_brush, c.Bounds, fmt);
                c = hdr_col[i];
                g.FillRectangle(c.Background, c.Bounds);
                g.DrawRectangle(p, c.Bounds);
                g.DrawString(c.DisplayValue, hdr_font, hdr_brush, c.Bounds, fmt);
            }

            /// Draw cells            
            for (int i = 0; i < CELL_N; ++i)
                for (int j = 0; j < CELL_N; ++j)
                {
                    c = cells[i][j];
                    g.FillRectangle(c.Background, c.Bounds);
                    g.DrawRectangle(p, c.Bounds);
                    g.DrawString(c.DisplayValue, cell_font, c.Foreground, c.Bounds, fmt);
                }
        }
        
        /// <summary>
        /// User drawn control - handle resizes here...
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // Ensure we are a square
            if (Width != Height)
            {
                int sz = Math.Min(Width, Height) + 1;
                Height = Width = sz;
            }

            // Cell initialized ??
            if (cells[0][0] == null)
                return; // No

            // Yes
            cell_size = Width / (CELL_N + 1);
            for (int i = 0; i < CELL_N; ++i)
            {
                for (int j = 0; j < CELL_N; ++j)
                {
                    cells[i][j].Size = cell_size;
                    cells[i][j].X = cell_size * (i + 1);
                    cells[i][j].Y = cell_size * (j + 1);
                }
            }

            for (int i = 0; i < CELL_N; ++i)
            {
                hdr_row[i].Size = cell_size;
                hdr_row[i].X = cell_size * (i + 1);
                hdr_row[i].Y = 0;
                hdr_col[i].Size = cell_size;
                hdr_col[i].X = 0;
                hdr_col[i].Y = cell_size * (i + 1);
            }

            hdr_font_size = cell_size * 0.5f;
            cell_font_size = 12f;
            hdr_font = new Font(hdr_font_name, hdr_font_size, FontStyle.Bold);
            cell_font = new Font(cell_font_name, cell_font_size, FontStyle.Bold);
        }

        /// <summary>
        /// Use to detect when a cell is clicked and propogate the event
        /// to any registered delegates.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int _x = e.X, _y = e.Y;

            // Find Cell that was clicked
            GridCell hit = null;
            for (int i = 0; i < CELL_N; ++i)
                for (int j = 0; j < CELL_N; ++j)
                    if (cells[i][j].HitTest(_x, _y))
                    {
                        hit = cells[i][j];
                        break;
                    }
            
            HandClicked handler = HandClickedEvent;
            if (handler != null && hit != null)
                handler(this, hit);
        }

        public IEnumerator<GridCell> GetEnumerator()
        {
            for (int i = 0; i < CELL_N; ++i)
                for (int j = 0; j < CELL_N; ++j)
                    yield return cells[i][j];
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }

    /// <summary>
    /// Factory class which provides enumerations of common 2 card hand groups.
    /// </summary>
    public class HandEnumerations
    {
        public static IEnumerable<string> PocketPairs
        {
            get
            {
                return
                    from s in Hands.hands
                    where s[0] == s[1]
                    select s;
            }
        }

        public static IEnumerable<string> AllBroadWay
        {
            get
            {
                return
                from s in Hands.hands
                where (s[0] != s[1]) && (rankOf(s[0]) >= rankOf('T')) && (rankOf(s[1]) >= rankOf('T'))
                select s;
            }
        }

        public static IEnumerable<string> SuitedConnectors(char low)
        {
            return
            from s in Hands.hands
                where ((s.Length == 3) && (s[2] == 's') && (rankOf(s[0]) >= rankOf(low) && rankOf(s[1]) >= rankOf(low)) &&
                        (rankOf(s[0]) - rankOf(s[1]) == 1))
                select s;
        }

        public static IEnumerable<string> SuitedGapper(char low, uint gap)
        {
            return
            from s in Hands.hands
            where ((s.Length == 3) && (s[2] == 's') && (rankOf(s[0]) >= rankOf(low) && rankOf(s[1]) >= rankOf(low)) &&
                    (rankOf(s[0]) - rankOf(s[1]) == (gap+1)))
            select s;
        }

        private static int rankOf(char c)
        {
            int rval = 12;
            foreach (char r in Hands.ranks)
                if (r == c)
                    break;
                else
                    --rval;
            return rval;
        }
    }

    /// <summary>
    /// A cell in the grid which represents a generic 2 card hand.
    /// Each cell has foreground, background and display value properties
    /// that are settable by the user.
    /// </summary>
    public class GridCell
    {
        private string _forHand;
        private string _displayValue = "";
        private Brush _background = Brushes.WhiteSmoke;
        private Brush _foreground = Brushes.Black;
        private int _x, _y, _size;
        private bool _sel = false;

        /// <summary>
        /// A 'selected' flag for this cell.
        /// </summary>
        public bool Selected
        {
            get { return _sel; }
            set { _sel = value; }
        }

        /// <summary>
        /// The Brush with which to render the background of the cell.
        /// </summary>
        public Brush Background
        {
            get { return _background; }
            set { _background = value; }
        }

        /// <summary>
        /// The Brush with which to render the foreground of the cell
        /// </summary>
        public Brush Foreground
        {
            get { return _foreground; }
            set { _foreground = value; }
        }

        /// <summary>
        /// The x-value of the upper left corner of the cell.
        /// </summary>
        public int X
        {
            get { return _x; }
            internal set { _x = value; }
        }

        /// <summary>
        /// The y-value of the upper left corner of the cell.
        /// </summary>
        public int Y
        {
            get { return _y; }
            internal set { _y = value; }
        }

        /// <summary>
        /// The value displayed in the cell.
        /// </summary>
        public string DisplayValue
        {
            get { return _displayValue; }
            set { _displayValue = value; }
        }

        /// <summary>
        /// The 2 card poker hand represented by this cell,
        /// ie "AA" or "K9s" or "T4o"
        /// </summary>
        public string ForHand
        {
            get { return _forHand; }
            internal set { _forHand = value; }
        }

        /// <summary>
        /// The size (height and width) of the cell.
        /// </summary>
        public int Size
        {
            get { return _size; }
            internal set { _size = value; }
        }
        
        /// <summary>
        /// Returns true iff the parameters are within the bounds of this cell
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        internal bool HitTest(int x, int y)
        {
            return ((x > _x) && (x < (_x + _size)) && (y > _y) && (y < (_y + _size))); 
        }
        
        /// <summary>
        /// The bounding rectangle for this cell.
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(_x, _y, _size, _size);
            }
        }
    }

    /// <summary>
    /// Utility methods for converting between [row,col] and hand strings, ie: 0,0 = AA: 0,1 = AKs: 1,0 = AKo: 12,12 = 22
    /// </summary>
    public class Hands
    {
        public static int nFor(string hand)
        {
            int rval = 0;
            foreach (string h in hands)
                if (hand.Equals(h))
                    break;
                else
                    ++rval;
            return rval;
        }

        public static string forN(int n)
        {
            for (int i = 0; i < n; ++i)
                if (i == n)
                    return hands[i];
            return null;
        }

        public static string handFor(int row, int col)
        {
            return hands[col * 13 + row];
        }

        public static void RowColFor(string hand, out int row, out int col)
        {
            int idx = nFor(hand);
            col = idx / 13;
            row = (idx % 13);
        }

        public static int indexFor(char rank)
        {
            int rval = 0;
            foreach (char r in ranks)
                if (r == rank)
                    break;
                else
                    ++rval;
            return rval;
        }

        public static char[] ranks = { 'A', 'K', 'Q', 'J', 'T', '9', '8', '7', '6', '5', '4', '3', '2' };

        public static string[] hands = {			
			"AA","AKs","AQs","AJs","ATs","A9s","A8s","A7s","A6s","A5s","A4s","A3s","A2s",
			"AKo","KK","KQs","KJs","KTs","K9s","K8s","K7s","K6s","K5s","K4s","K3s","K2s",
			"AQo","KQo","QQ","QJs","QTs","Q9s","Q8s","Q7s","Q6s","Q5s","Q4s","Q3s","Q2s",
			"AJo","KJo","QJo","JJ","JTs","J9s","J8s","J7s","J6s","J5s","J4s","J3s","J2s",
			"ATo","KTo","QTo","JTo","TT","T9s","T8s","T7s","T6s","T5s","T4s","T3s","T2s",
			"A9o","K9o","Q9o","J9o","T9o","99","98s","97s","96s","95s","94s","93s","92s",
			"A8o","K8o","Q8o","J8o","T8o","98o","88","87s","86s","85s","84s","83s","82s",
			"A7o","K7o","Q7o","J7o","T7o","97o","87o","77","76s","75s","74s","73s","72s",
			"A6o","K6o","Q6o","J6o","T6o","96o","86o","76o","66","65s","64s","63s","62s",
			"A5o","K5o","Q5o","J5o","T5o","95o","85o","75o","65o","55","54s","53s","52s",
			"A4o","K4o","Q4o","J4o","T4o","94o","84o","74o","64o","54o","44","43s","42s",
			"A3o","K3o","Q3o","J3o","T3o","93o","83o","73o","63o","53o","43o","33","32s",
			"A2o","K2o","Q2o","J2o","T2o","92o","82o","72o","62o","52o","42o","32o","22"};
    }
}
