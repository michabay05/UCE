﻿namespace Nuclear.src;

class Attack
{
    public static ulong[,] pawnAttacks;
    public static ulong[] knightAttacks;
    public static ulong[] kingAttacks;
    public static ulong[] bishopOccMask;
    public static ulong[,] bishopAttacks;
    public static ulong[] rookOccMask;
    public static ulong[,] rookAttacks;

    public static readonly int[] bishopRelevantBits =
    {
        6, 5, 5, 5, 5, 5, 5, 6,
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 9, 9, 7, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5,
        6, 5, 5, 5, 5, 5, 5, 6
    };
    public static readonly int[] rookRelevantBits =
    {
        12, 11, 11, 11, 11, 11, 11, 12,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        11, 10, 10, 10, 10, 10, 10, 11,
        12, 11, 11, 11, 11, 11, 11, 12
    };

    static Attack()
    {
        pawnAttacks = new ulong[2, 64];
        knightAttacks = new ulong[64];
        kingAttacks = new ulong[64];
        bishopOccMask = new ulong[64];
        bishopAttacks = new ulong[64, 512];
        rookOccMask = new ulong[64];
        rookAttacks = new ulong[64, 4096];
    }

    public static void Init()
    {
        InitLeapers();
        InitSliding(Piece.BISHOP);
        InitSliding(Piece.ROOK);
    }

    private static void InitLeapers()
    {
        for (int sq = 0; sq < 64; sq++)
        {
            GenPawnAttacks(0, sq);
            GenPawnAttacks(1, sq);
            GenKnightAttacks(sq);
            GenKingAttacks(sq);
        }
    }

    private static void InitSliding(int piece)
    {
        if (piece != (int)Piece.BISHOP && piece != (int)Piece.ROOK)
            return;
        for (int sq = 0; sq < 64; sq++)
        {
            bishopOccMask[sq] = GenBishopOccupancy(sq);
            rookOccMask[sq] = GenRookOccupancy(sq);
            ulong currentMask = (piece == (int)Piece.BISHOP) ? bishopOccMask[sq] : rookOccMask[sq];
            int bitCount = BitUtil.CountBits(currentMask);
            for (int count = 0; count < (1 << bitCount); count++)
            {
                ulong occupancy = setOccupancy(count, bitCount, currentMask);
                int magicInd;
                if (piece == (int)Piece.BISHOP)
                {
                    magicInd = (int)((occupancy * Magics.bishopMagics[sq]) >> (64 - bitCount));
                    bishopAttacks[sq, magicInd] = GenBishopAttacks(sq, occupancy);
                }
                else
                {
                    magicInd = (int)((occupancy * Magics.rookMagics[sq]) >> (64 - bitCount));
                    rookAttacks[sq, magicInd] = GenRookAttacks(sq, occupancy);
                }
            }
        }
    }

    public static ulong GetAttack(int piece, int sq, ulong blockerBoard)
    {
        switch (piece % 6)
        {
            case 0:
                return pawnAttacks[0, sq];
            case 1:
                return knightAttacks[sq];
            case 2:
                return Magics.GetBishopAttack(sq, blockerBoard);
            case 3:
                return Magics.GetRookAttack(sq, blockerBoard);
            case 4:
                return Magics.GetQueenAttack(sq, blockerBoard);
            case 6:
                return kingAttacks[sq];
            default:
                return 0UL;
        }
    }

    public static void GenPawnAttacks(int color, int sq)
    {
        if (color == 0) // WHITE
        {
            if (BoardUtil.GetRank(sq) < 7 && BoardUtil.GetFile(sq) > 0)
                BitUtil.SetBit(ref pawnAttacks[color, sq], sq + (int)Direction.SW);
            if (BoardUtil.GetRank(sq) < 7 && BoardUtil.GetFile(sq) < 7)
                BitUtil.SetBit(ref pawnAttacks[color, sq], sq + (int)Direction.SE);
        }
        else // BLACK
        {
            if (BoardUtil.GetRank(sq) > 0 && BoardUtil.GetFile(sq) > 0)
                BitUtil.SetBit(ref pawnAttacks[color, sq], sq + (int)Direction.NW);
            if (BoardUtil.GetRank(sq) > 0 && BoardUtil.GetFile(sq) < 7)
                BitUtil.SetBit(ref pawnAttacks[color, sq], sq + (int)Direction.NE);
        }
    }

    public static void GenKnightAttacks(int sq)
    {
        // TODO: KNIGHT MOVES FOR G1 - NOT WORKING
        if (BoardUtil.GetRank(sq) <= 5 && BoardUtil.GetFile(sq) >= 1)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.SW_S);  // - 17
        if (BoardUtil.GetRank(sq) <= 6 && BoardUtil.GetFile(sq) >= 2)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.SW_W);  // - 10
        if (BoardUtil.GetRank(sq) <= 6 && BoardUtil.GetFile(sq) <= 5)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.SE_E);  // - 6
        if (BoardUtil.GetRank(sq) <= 5 && BoardUtil.GetFile(sq) <= 6)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.SE_S);  // - 15
        if (BoardUtil.GetRank(sq) >= 2 && BoardUtil.GetFile(sq) <= 6)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.NE_N);  // + 17
        if (BoardUtil.GetRank(sq) >= 1 && BoardUtil.GetFile(sq) <= 5)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.NE_E);  // + 10
        if (BoardUtil.GetRank(sq) >= 1 && BoardUtil.GetFile(sq) >= 2)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.NW_W);  // + 6
        if (BoardUtil.GetRank(sq) >= 2 && BoardUtil.GetFile(sq) >= 1)
            BitUtil.SetBit(ref knightAttacks[sq], sq + (int)Direction.NW_N);  // + 15
    }

    public static void GenKingAttacks(int sq)
    {
        if (BoardUtil.GetRank(sq) > 0)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.N);
        if (BoardUtil.GetRank(sq) < 7)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.S);
        if (BoardUtil.GetFile(sq) > 0)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.W);
        if (BoardUtil.GetFile(sq) < 7)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.E);
        if (BoardUtil.GetRank(sq) > 0 && BoardUtil.GetFile(sq) > 0)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.NW);
        if (BoardUtil.GetRank(sq) > 0 && BoardUtil.GetFile(sq) < 7)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.NE);
        if (BoardUtil.GetRank(sq) < 7 && BoardUtil.GetFile(sq) > 0)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.SW);
        if (BoardUtil.GetRank(sq) < 7 && BoardUtil.GetFile(sq) < 7)
            BitUtil.SetBit(ref kingAttacks[sq], sq + (int)Direction.SE);
    }

    /* Generates the block mask for the bishop */
    public static ulong GenBishopOccupancy(int sq)
    {
        ulong bishopOcc = 0;
        int r, sr = sq / 8;  // { current rank }, { target rank }
        int f, sf = sq % 8;  // { current file }, { target file }
        for (r = sr + 1, f = sf + 1; r < 7 && f < 7; r++, f++)
            BitUtil.SetBit(ref bishopOcc, (r * 8) + f);
        for (r = sr + 1, f = sf - 1; r < 7 && f > 0; r++, f--)
            BitUtil.SetBit(ref bishopOcc, (r * 8) + f);
        for (r = sr - 1, f = sf + 1; r > 0 && f < 7; r--, f++)
            BitUtil.SetBit(ref bishopOcc, (r * 8) + f);
        for (r = sr - 1, f = sf - 1; r > 0 && f > 0; r--, f--)
            BitUtil.SetBit(ref bishopOcc, (r * 8) + f);
        return bishopOcc;
    }

    /* Generates the block mask for the rook */
    public static ulong GenRookOccupancy(int sq)
    {
        ulong rookOcc = 0;
        int r, sr = sq / 8;  // { current rank }, { target rank }
        int f, sf = sq % 8;  // { current file }, { target file }
        for (r = sr + 1; r < 7; r++)
            BitUtil.SetBit(ref rookOcc, (r * 8) + sf);
        for (r = sr - 1; r > 0; r--)
            BitUtil.SetBit(ref rookOcc, (r * 8) + sf);
        for (f = sf - 1; f > 0; f--)
            BitUtil.SetBit(ref rookOcc, (sr * 8) + f);
        for (f = sf + 1; f < 7; f++)
            BitUtil.SetBit(ref rookOcc, (sr * 8) + f);
        return rookOcc;
    }

    public static ulong GenBishopAttacks(int sq, ulong blockBoard)
    {
        ulong bishopOcc = 0;
        int r, sr = sq / 8;  // { current rank }, { target rank }
        int f, sf = sq % 8;  // { current file }, { target file }
        for (r = sr + 1, f = sf + 1; r <= 7 && f <= 7; r++, f++)
        {
            int currSq = (r * 8) + f;
            BitUtil.SetBit(ref bishopOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        for (r = sr + 1, f = sf - 1; r <= 7 && f >= 0; r++, f--)
        {
            int currSq = (r * 8) + f;
            BitUtil.SetBit(ref bishopOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        for (r = sr - 1, f = sf + 1; r >= 0 && f <= 7; r--, f++)
        {
            int currSq = (r * 8) + f;
            BitUtil.SetBit(ref bishopOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        for (r = sr - 1, f = sf - 1; r >= 0 && f >= 0; r--, f--)
        {
            int currSq = (r * 8) + f;
            BitUtil.SetBit(ref bishopOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        return bishopOcc;
    }

    public static ulong GenRookAttacks(int sq, ulong blockBoard)
    {
        ulong rookOcc = 0;
        int r, sr = sq / 8;  // { current rank }, { target rank }
        int f, sf = sq % 8;  // { current file }, { target file }
        for (r = sr + 1; r <= 7; r++)
        {
            int currSq = (r * 8) + sf;
            BitUtil.SetBit(ref rookOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        for (r = sr - 1; r >= 0; r--)
        {
            int currSq = (r * 8) + sf;
            BitUtil.SetBit(ref rookOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        for (f = sf - 1; f >= 0; f--)
        {
            int currSq = (sr * 8) + f;
            BitUtil.SetBit(ref rookOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        for (f = sf + 1; f <= 7; f++)
        {
            int currSq = (sr * 8) + f;
            BitUtil.SetBit(ref rookOcc, currSq);
            if (BitUtil.GetBit(blockBoard, currSq)) break;
        }
        return rookOcc;
    }

    public static ulong setOccupancy(int index, int relevant_bits, ulong attack_mask)
    {
        ulong occupancy = 0;
        for (int count = 0; count < relevant_bits; count++)
        {
            int ls1bIndex = BitUtil.GetLs1bIndex(attack_mask);
            // Remove the least significant bit
            BitUtil.PopBit(ref attack_mask, ls1bIndex);
            if ((index & (1 << count)) > 0) BitUtil.SetBit(ref occupancy, ls1bIndex);
        }
        return occupancy;
    }
}
