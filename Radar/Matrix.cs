using System;
using System.Globalization;
using System.Threading;

namespace Radar
{
    /// <summary>
    /// Klasa reprezentująca macierz dwuwymiarową liczb typu <see cref="double"/>. Umożliwia wykonywanie podstawowych działań na macierzach, obliczanie wyznacznika, wyznaczanie macierzy transponowanej.
    /// </summary>
    ///
    public class Matrix
    {

        //  WŁAŚCIWOŚCI

        /// <summary>Dwuwymiarowa macierz liczb typu <see cref="double"/></summary>
        /// 
        public double[,] Array {  get; private set; }

        /// <summary>Liczba wierszy w macierzy.</summary>
        /// 
        public int Rows { get; }

        /// <summary>Liczba kolumn w macierzy.</summary>
        /// 
        public int Columns { get; }

        /// <summary>Macierz transponowana.</summary>
        /// 
        public Matrix T
        {
            get
            {
                Matrix matrix = new(Columns, Rows);
                for (int i = 0; i < Rows; i++) 
                    for (int j = 0; j < Columns; j++)
                    {
                        matrix[j, i] = this[i, j];
                    }

                return matrix;
            }
        }

        /// <summary>Macierz dopełnień algenbraicznych.</summary>
        /// 
        public Matrix D
        {
            get
            {
                Matrix matrix = new(Columns, Rows);

                for (int i = 0; i < Rows; i++)
                    for (int j = 0; j < Columns; j++)
                    {
                        matrix[i, j] = Math.Pow(-1, i + j) * Det(Delete(i, j));
                    }

                return matrix;
            }
        }


        //  KONSTRUKTORY

        /// <summary>Tworzy macierz <see cref="Matrix"/> na podstawie zadanej macierzy dwuwymiarowej <paramref name="array"/>.</summary>
        /// <param name="array">Macierz dwuwymiarowa na podstawie której inicjujemy naszą klasę.</param>
        /// 
        public Matrix(double[,] array)
        {
            Array = array;
            Rows = array.GetLength(0);
            Columns = array.GetLength(1);
        }

        /// <summary>Tworzy losową macierz o zadanej liczbie wierszy <paramref name="rows"/> oraz kolumn <paramref name="columns"/>.</summary>
        /// <param name="rows">Liczba wierszy macierzy.</param>
        /// <param name="columns">Liczba kolumn macierzy.</param>
        /// 
        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Array = new double[Rows, Columns];
        }

        /// <summary>Tworzy bezwymiarową macierz.</summary>
        /// 
        public Matrix()
        {
            Array = new double[,] { };
            Rows = 0;
            Columns = 0;
        }

        
        //  METODY

        /// <summary>Oblicza wyznacznik macierzy jeżeli <paramref name="m"/> jest macierzą kwadratową. W przeciwnym razie podnosi wyjątek.</summary>
        /// <param name="m">Macierz, której wyznacznik chcemy obliczyć.</param>
        /// <returns>Wyznacznik macierzy <paramref name="m"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public static double Det(Matrix m)
        {

            //  Jeżeli macierz jest kwadratowa.
            if (m.IsSquare())
            {

                //  W zależności od rozmiaru macierzy przyjmujemy różne metody 
                //  obliczania wyznacznika. 
                if (m.Rows == 1)
                {
                    return m[0, 0];
                }
                else if (m.Rows == 2)
                {
                    return m[0,0] * m[1,1] - m[0,1] * m[1,0];
                }
                else if (m.Rows == 3)
                {
                    double sum = 0;
                    sum += m[0, 0] * m[1, 1] * m[2, 2];
                    sum += m[0, 1] * m[1, 2] * m[2, 0];
                    sum += m[0, 2] * m[1, 0] * m[2, 1];
                    sum -= m[0, 2] * m[1, 1] * m[2, 0];
                    sum -= m[0, 0] * m[1, 2] * m[2, 1];
                    sum -= m[0, 1] * m[1, 0] * m[2, 2];

                    return sum;
                }
                else
                {
                    double sum = 0;

                    for (int i = 0; i < m.Rows; i++)
                    {
                        Matrix matrix = m.Delete(i, 0);

                        sum += Math.Pow(-1, i) * m[i, 0] * Det(matrix);
                    }

                    return sum;
                }
            }
            //  Jeżeli macierz nie jest kwadratowa.
            else
                throw new ArgumentException("Matrix is not square.");
        }

        /// <summary>Wyznacza nową macierz poprzez usunięcie wiersza <paramref name="row"/> oraz kolumny <paramref name="column"/>.</summary>
        /// <remarks>Potrzebne na przykład przy obliczaniu wyznacznika macierzy.</remarks>
        /// <param name="row">Indeks wiersza, który ma zostać usunięty.</param>
        /// <param name="column">Indeks kolumny, która ma zostać usunięta.</param>
        /// <returns>Nową, pomniejszoną o jeden wiersz i jedną kolumnę macierz.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public Matrix Delete(int row, int column)
        {
            Matrix m;

            if (0 <= row && 0 <= column)
            {
                m = new(Rows - 1, Columns - 1);
                int ii = 0;
                int jj = 0;

                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        if (!(i == row || j == column))
                        {
                            m.Array[ii, jj] = Array[i, j];
                            jj++;
                        }

                    }

                    jj = 0;

                    if (!(i == row))
                        ii++;
                }

                return m;
            }
            else
                throw new ArgumentException("Invalid input parameters: row and column must be non-negative integer");
            
        }

        /// <summary>Tworzy głęboką kopię macierzy <see cref="Matrix"/>.</summary>
        /// <returns>Głęboką kopię macierzy <see cref="Matrix"/>.</returns>
        /// 
        public Matrix DeepCopy()
        {
            //  Aby stworzyć głęboką kopię macierzy należy stworzyć płytką kopię tejże
            //  macierzy oraz płytką kopię tablicy Array. 
            Matrix matrix = (Matrix)MemberwiseClone();
            matrix.Array = (double[,])Array.Clone();

            return matrix;
        }

        /// <summary>Sprawdza, czy macierz jest kwadratowa.</summary>
        /// <returns>
        /// <para><c>true</c> jeżeli macierz jest kwadratowa.</para>
        /// <para><c>false</c> w przeciwnym razie.</para>
        /// </returns>
        /// 
        public bool IsSquare()
        {
            if (Columns == Rows)
                return true;
            else
                return false;
        }

        /// <summary>Sprawdza, czy macierz <paramref name="matrix"/> jest takich samych rozmiarów co nasza instancja.</summary>
        /// <param name="matrix">Macierz, której rozmiar porównujemy.</param>
        /// <returns>
        /// <para><c>True</c> jeżeli macierze mają taki sam rozmiar.</para>
        /// <para><c>False</c> w przeciwnym razie.</para>
        /// </returns>
        /// 
        public bool IsSameSize(Matrix matrix)
        {
            if (Rows == matrix.Rows && Columns == matrix.Columns)
                return true;
            else
                return false;
        }


        //  METODY STATYCZNE

        /// <summary>Generuje macierz jednostkową o zadanym rozmiarze <paramref name="size"/>.</summary>
        /// <param name="size">Rozmiar generowanej macierzy.</param>
        /// <returns>Macierz jednostkową o rozmiarze <paramref name="size"/>.</returns>
        /// 
        public static Matrix Identity(int size)
        {
            Matrix output = new(size, size);
            output *= 0;

            for (int i = 0; i < size; i++)
                output.Array[i, i] = 1.0;

            return output;
        }


        //  OPERATORY

        /// <summary>Określa operator potęgowania macierzy <paramref name="left"/> do potęgi <paramref name="count"/>.</summary>
        /// <param name="left">Macierz <see cref="Matrix"/> podnoszona do potęgi.</param>
        /// <param name="count">Wykładnik potęgowania.</param>
        /// <returns>Obiekt typu <see cref="Matrix"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public static Matrix operator ^(Matrix left, int count)
        {
            if (count == 0) {  return left.DeepCopy(); }
            else if (count == 1) {  return left.DeepCopy(); }
            else if (count < 0)
            {
                int multiply = Math.Abs(count);
                Matrix matrix = left.DeepCopy();
                double det = Det(matrix);

                if (det != 0)
                {
                    matrix = matrix.D.T * (1.0 / Det(matrix));
                }
                else
                    throw new ArgumentException("Determinant of the matrix is equal to 0");

                if (1 < multiply)
                    matrix ^= multiply;

                return matrix;
            }
            else if (1 < count)
            {
                Matrix matrix = left.DeepCopy();

                for (int i = 1; i < count; i++)
                {
                    matrix *= left;
                }

                return matrix;
            }
            else
                throw new ArgumentException("Invalid arguments");
        }

        /// <summary>Określa operator mnożenia macierzy <paramref name="left"/> przez macierz <paramref name="right"/>.</summary>
        /// <param name="left">Macierz po lewej stronie operatora.</param>
        /// <param name="right">Macierz po prawej stronie operatora.</param>
        /// <returns>Obiekt typu <see cref="Matrix"/></returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public static Matrix operator *(Matrix left, Matrix right)
        {
            if (left.Columns == right.Rows)
            {
                Matrix matrix = new(left.Rows, right.Columns);
                double sum = 0.0;

                for (int i = 0; i < left.Rows; i++)
                    for (int j = 0; j < right.Columns; j++)
                    {
                        for (int k = 0; k < left.Columns; k++)
                        {
                            sum += left[i, k] * right[k, j];
                        }

                        matrix[i, j] = sum;
                        sum = 0.0;
                    }

                return matrix;
            }
            else
                throw new ArgumentException("Invalid matrix sizes");
        }

        /// <summary>Określa operator mnożenia macierzy <paramref name="left"/> przez liczbę zmiennoprzecinkową <paramref name="right"/>.</summary>
        /// <param name="left">Macierz po lewej stronie operatora.</param>
        /// <param name="right">Liczba zmiennoprzecinkowa po prawej stronie operatora.</param>
        /// <returns>Obiekt typu <see cref="Matrix"/>.</returns>
        /// 
        public static Matrix operator *(Matrix left, double right)
        {
            Matrix matrix = new(left.Rows, left.Columns);

            for (int i = 0; i < left.Rows; i++)
                for (int j = 0; j < left.Columns; j++)
                {
                    matrix[i, j] = left[i, j] * right;
                }

            return matrix;
        }

        /// <summary>Określa operator mnożenia liczby zmiennoprzecinkowej <paramref name="left"/> przez macierz <paramref name="right"/>.</summary>
        /// <param name="left">Liczba zmiennoprzecinkowa po lewej stronie operatora.</param>
        /// <param name="right">Macierz po prawej stronie operatora.</param>
        /// <returns>Obiekt typu <see cref="Matrix"/>.</returns>
        /// 
        public static Matrix operator *(double left, Matrix right)
        {
            Matrix matrix = new(right.Rows, right.Columns);

            for (int i = 0; i < right.Rows; i++)
                for (int j = 0; j < right.Columns; j++)
                {
                    matrix[i, j] = right[i, j] * left;
                }

            return matrix;
        }

        /// <summary>Określa operator dodawania macierzy. Macierze muszą być tych samych rozmiarów.</summary>
        /// <param name="left">Macierz po lewej stronie operatora.</param>
        /// <param name="right">Macierz po prawej stronie operatora.</param>
        /// <returns>Obiekt typu <see cref="Matrix"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public static Matrix operator +(Matrix left, Matrix right)
        {
            if (left.IsSameSize(right))
            {
                Matrix matrix = new(left.Rows, left.Columns);

                for (int i = 0; i < left.Rows; i++)
                    for (int j = 0; j < left.Columns; j++)
                    {
                        matrix[i, j] =  left[i, j] + right[i, j];
                    }

                return matrix;
            }
            else
                throw new ArgumentException("Invalid matrix sizes");
        }

        /// <summary>Określa operator odejmowania macierzy. Macierze muszą być tych samych rozmiarów.</summary>
        /// <param name="left">Macierz po lewej stronie operatora.</param>
        /// <param name="right">Macierz po prawej stronie operatora.</param>
        /// <returns>Obiekt typu <see cref="Matrix"/>.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// 
        public static Matrix operator -(Matrix left, Matrix right)
        {
            if (left.IsSameSize(right))
            {
                Matrix matrix = new(left.Rows, left.Columns);

                for (int i = 0; i < left.Rows; i++)
                    for (int j = 0; j < left.Columns; j++)
                    {
                        matrix[i, j] = left[i, j] - right[i, j];
                    }

                return matrix;
            }
            else
                throw new ArgumentException("Invalid matrix sizes");
        }

        /// <summary>Rzutuje macierz <see cref="Matrix"/> na reprezentację tekstową typu <see cref="string"/>.</summary>
        /// <param name="m">Obiekt typu <see cref="Matrix"/>.</param>
        /// <returns>Tekstową reprezentację macierzy.</returns>
        /// 
        public static implicit operator string(Matrix m)
        {
            string str = string.Empty;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            for (int i = 0; i < m.Rows; i++)
            {
                for (int j = 0; j < m.Columns; j++)
                {
                    str += m[i, j].ToString("##0.###") + "\t";
                }

                str += "\n";
            }

            return str;
        }


        //  INDEKSERY

        /// <summary>Podstawowy indekser. Pobiera lub ustawia liczbę zmiennoprzecinkową znajdującą się na pozycji określonej przez numer wiersza <paramref name="i"/> oraz numer kolumny <paramref name="j"/>.</summary>
        /// <param name="i">Numer wiersza.</param>
        /// <param name="j">Numer kolumny.</param>
        /// <returns>Liczbę znajdującą się na pozycji określonej przez współrzędne <paramref name="i"/> oraz <paramref name="j"/>.</returns>
        /// 
        public double this[int i, int j] 
        {
            get { return Array[i, j]; }
            set { Array[i, j] = value; }
        }

        /// <summary>Indekser dla macierzy o jednym wierszu lub jednej kolumnie.</summary>
        /// <remarks>Jeżeli macierz posiada tylko jedną kolumnę lub tylko jeden wiersz, to możemy się odwoływać do elementów powstałego w ten sposób wektora poprzez jedną współrzędną.</remarks>
        /// <param name="i">Indeks elementu w wektorze.</param>
        /// <returns>Liczbę zmiennoprzecinkową znajdującą się pod danym indeksem.</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// 
        public double this[int i]
        {
            get
            {
                if (Rows == 1) return Array[0, i];
                else if (Columns == 1) return Array[i, 0];
                else throw new NotImplementedException("This is not a one dimensional array");
            }
            set
            {
                if (Rows == 1) Array[0, i] = value;
                else if (Columns == 1) Array[i, 0] = value;
                else throw new NotImplementedException("This is not a one dimensional array");
            }
        }

        /// <summary>NIE DZIAŁA! Indekser eksperymentalny i testowy.</summary>
        /// 
        public double this[Range i, Range j]
        {
            get
            {
                return Array[i.Start.Value, j.Start.Value];
            }
            set
            {
                var a = Array[1, 0];
                Array[i.Start.Value, j.Start.Value] = value;
            }
        }

        /// <summary>NIE DZIAŁA! Indekser eksperymentalny i testowy.</summary>
        /// 
        public double this[Index i, Index j]
        {
            get
            {
                i = ^2;
                
                return Array[i.Value, j.Value];
            }
            set
            {
                var a = Array[1, 0];
                Array[i.Value, j.Value] = value;
            }
        }


        //  FUNKCJE NADPISANE

        /// <summary>Generuje tekstową reprezentację macierzy.</summary>
        /// <returns>Obiekt typu <see cref="string"/>.</returns>
        /// 
        public override string ToString()
        {
            return (string)this;
        }
    }
}
