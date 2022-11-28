namespace TicTacToeOnline.Shared.Models
{
    public class Room
    {
        public string Id { get; set; } = default!;
        public string RoomName { get; set; } = default!;
        public List<User> Users { get; set; } = new();
        public string MovingUser { get; set; } = default!;
        public List<Figure> Figures { get; set; } = new();

        public int ScorePlayer1 { get; set; }
        public int ScorePlayer2 { get; set; }
        public int RoundCount { get; set; } 

        public void ResetFigures()
        {
            Figures.Clear();
            for (int i = 0; i < 9; i++)
            {
                Figures.Add(new Figure());
            }
        }

        public bool CheckPlayerWin(FigureType figureType)
        {
            if (figureType == FigureType.None)
                return false;

            for (int i = 0; i < 9; i += 3)
            {
                if (Figures[i].FigureType == figureType 
                    && Figures[i + 1].FigureType == figureType 
                    && Figures[i + 2].FigureType == figureType)
                    return true;
            }

            for (int i = 0; i < 3; i++)
            {
                if (Figures[i].FigureType == figureType
                    && Figures[i + 3].FigureType == figureType
                    && Figures[i + 6].FigureType == figureType)
                    return true;
            }

            if (Figures[0].FigureType == figureType
                && Figures[4].FigureType == figureType
                && Figures[8].FigureType == figureType)
                return true;

            if (Figures[2].FigureType == figureType
                && Figures[4].FigureType == figureType
                && Figures[6].FigureType == figureType)
                return true;

            return false;
        }

        public bool CheckIfAllCellsFilled()
        {
            for (int i = 0; i < Figures.Count; i++)
            {
                if (Figures[i].FigureType == FigureType.None)
                    return false;
            }

            return true;
        }
    }
}
