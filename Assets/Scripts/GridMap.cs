using System;

public class GridMap<T>
{
    private T[,] gridArray;
    public T this[int x, int y]
    {
        get { return gridArray[x, y]; }
        set { gridArray[x, y] = value; }
    }

    public int height
    {
        get { return gridArray.GetLength(1); }
    }
    public int width
    {
        get { return gridArray.GetLength(0); }
    }

    public GridMap(int w, int h)
    {
        if (w == 0 || h == 0)
            throw new ArgumentException("Width or height cannot be 0");

        gridArray = new T[w, h];
    }

    public T[] GetColumn(int index)
    {
        T[] column = new T[height-1];
        for (int i = 0; i < height; i++)
        {
           column[i] = gridArray[index, i];
        }

        return column;
    }

    public T[] GetRow(int index)
    {
        T[] row = new T[width - 1];
        for (int i = 0; i < width; i++)
        {
            row[i] = gridArray[i, index];
        }

        return row;
    }
}
