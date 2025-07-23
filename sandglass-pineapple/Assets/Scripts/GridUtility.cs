using UnityEngine;
using UnityEngine.UI;

public static class GridUtility
{
    public static void FitSquareGrid(GridLayoutGroup glg, int rows, int cols)
    {
        if (rows <= 0 || cols <= 0)
        {
            Debug.LogWarning("GridUtility: Invalid row or column count.");
            return;
        }

        RectTransform rt = glg.GetComponent<RectTransform>();
        Rect rect = rt.rect;

        float spacingX = glg.spacing.x;
        float spacingY = glg.spacing.y;

        // Total spacing
        float totalSpacingX = spacingX * (cols - 1);
        float totalSpacingY = spacingY * (rows - 1);

        // Usable area
        float usableWidth = rect.width - glg.padding.left - glg.padding.right - totalSpacingX;
        float usableHeight = rect.height - glg.padding.top - glg.padding.bottom - totalSpacingY;

        // Final square cell size (smallest possible)
        float cellSize = Mathf.Floor(Mathf.Min(usableWidth / cols, usableHeight / rows));

        if (cellSize <= 0)
        {
            Debug.LogWarning("GridUtility: Cell size too small to layout.");
            return;
        }

        // Apply grid settings
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = cols;
        glg.cellSize = new Vector2(cellSize, cellSize);

        // Recalculate grid actual size
        float gridWidth = cellSize * cols + spacingX * (cols - 1);
        float gridHeight = cellSize * rows + spacingY * (rows - 1);

        // Center grid using padding
        float leftoverX = Mathf.Max(0f, rect.width - gridWidth);
        float leftoverY = Mathf.Max(0f, rect.height - gridHeight);

        int padLeft = Mathf.RoundToInt(leftoverX / 2f);
        int padRight = Mathf.RoundToInt(leftoverX / 2f);
        int padTop = Mathf.RoundToInt(leftoverY / 2f);
        int padBottom = Mathf.RoundToInt(leftoverY / 2f);

        glg.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
    }
}
