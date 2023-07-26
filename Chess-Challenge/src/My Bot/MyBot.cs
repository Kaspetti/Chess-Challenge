using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
        
    private Dictionary<string, int[,]> positionPoints = new Dictionary<string, int[,]>() {
        {"Pawn", new int[8,8] {
            {7,7,7,7,7,7,7,7},
            {6,6,6,6,6,6,6,6},
            {5,5,5,5,5,5,5,5},
            {4,4,4,4,4,4,4,4},
            {3,3,3,3,3,3,3,3},
            {2,2,2,2,2,2,2,2},
            {1,1,1,1,1,1,1,1},
            {0,0,0,0,0,0,0,0},
        }},
        {"Knight", new int[8,8] {
            {1,1,1,1,1,1,1,1},
            {1,3,3,3,3,3,3,1},
            {1,3,5,5,5,5,3,1},
            {1,3,5,7,7,5,3,1},
            {1,3,5,7,7,5,3,1},
            {1,3,5,5,5,5,3,1},
            {1,3,3,3,3,3,3,1},
            {1,1,1,1,1,1,1,1}
        }},
        {"Bishop", new int[8,8] {
            {7,5,3,1,1,3,5,7},
            {5,7,5,3,3,5,7,5},
            {3,5,7,5,5,7,5,3},
            {1,3,5,7,7,5,3,1},
            {1,3,5,7,7,5,3,1},
            {3,5,7,5,5,7,5,3},
            {5,7,5,3,3,5,7,5},
            {7,5,3,1,1,3,5,7},
        }},
        {"Rook", new int[8,8] {
            {7,7,7,7,7,7,7,7},
            {7,4,4,4,4,4,4,7},
            {7,4,1,1,1,1,4,7},
            {7,4,1,0,0,1,4,7},
            {7,4,1,0,0,1,4,7},
            {7,4,1,1,1,1,4,7},
            {7,4,4,4,4,4,4,7},
            {7,7,7,7,7,7,7,7}
        }},
        {"King", new int[8,8] {
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0},
            {0,1,1,1,1,1,1,0},
            {2,3,3,3,3,3,3,2},
            {4,5,5,5,5,5,5,4},
            {6,7,7,7,7,7,7,6},
            {6,6,6,6,6,6,6,6},
        }},
        {"Queen", new int[8,8] {
            {4,4,4,4,4,4,4,4},
            {4,5,5,5,5,5,5,4},
            {4,5,6,6,6,6,5,4},
            {4,5,6,7,7,6,5,4},
            {4,5,6,7,7,6,5,4},
            {4,5,6,6,6,6,5,4},
            {4,5,5,5,5,5,5,4},
            {4,4,4,4,4,4,4,4},
        }},
    };

    private Dictionary<string, int> pieceModifiers = new Dictionary<string, int>() {
        {"Pawn", 2},
        {"Bishop", 4},
        {"Rook", 4},
        {"Knight", 4},
        {"Queen", 8},
        {"King", 10},
    };

    private int depth = 1;

    public Move Think(Board board, Timer timer)
    {        
        return FindBestMove(board);
    }

    private Move FindBestMove(Board board) { 
        var moves = board.GetLegalMoves();

        var isWhite = board.IsWhiteToMove;

        var bestMove = moves[0];
        var bestScore = isWhite ? int.MinValue : int.MaxValue;

        foreach (var move in moves) {
            board.MakeMove(move);

            var currentScore = CalculateScore(board);
            var isWinning = (board.IsWhiteToMove && currentScore < 0) || (!board.IsWhiteToMove && currentScore > 0);

            if (board.IsRepeatedPosition() && isWinning) {
                board.UndoMove(move);
                continue;
            }

            if (board.IsInCheckmate()) {
                return move;
            }

            var opponentMoves = board.GetLegalMoves();
            if (opponentMoves.Length == 0 && isWinning) {
                board.UndoMove(move);
                continue;
            }

            var worstResponseScore = isWhite ? int.MaxValue : int.MinValue;
            foreach (var opponentMove in opponentMoves) {
                board.MakeMove(opponentMove);
                var score = CalculateScore(board);
                board.UndoMove(opponentMove);

                if (isWhite) {
                    worstResponseScore = Math.Min(score, worstResponseScore);
                } else {
                    worstResponseScore = Math.Max(score, worstResponseScore);
                }
            }

            if (isWhite) {
                if (bestScore < worstResponseScore) {
                    bestScore = worstResponseScore;
                    bestMove = move;
                }
            } else {
                if (bestScore > worstResponseScore) {
                    bestScore = worstResponseScore;
                    bestMove = move;
                }
            }

            board.UndoMove(move);
        }

        return bestMove;
    }


    private int CalculateScore(Board board) {
        var score = 0;

        for (int i = 0; i < 64; i++) {
            var square = new Square(i);
            var piece = board.GetPiece(square);

            var blackModifier = piece.IsWhite ? 1 : 0;

            if (piece.PieceType != PieceType.None) {
                score += (positionPoints[piece.PieceType.ToString()][
                    Math.Abs(square.Rank - (7 * blackModifier)),
                    Math.Abs(square.File - (7 * blackModifier))]
                + pieceModifiers[piece.PieceType.ToString()]) * (piece.IsWhite ? 1 : -1);
            }
        }

        return score;
    }
}
