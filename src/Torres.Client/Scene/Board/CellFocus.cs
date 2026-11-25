namespace Torres.Client.Scene
{
    internal readonly record struct CellFocus(BoardCell? HoveredCell, BoardCell? SelectedCell);
}
