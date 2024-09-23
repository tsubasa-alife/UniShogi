using Cysharp.Threading.Tasks;

namespace UniShogi
{
	/// <summary>
	/// NNUE評価関数によるエンジン
	/// </summary>
	public class NnueEngine : BaseEngine
	{
		private const int _boardSize = 9;
		/// <summary>
        /// P特徴量のインデックス
        /// </summary>
        private enum PieceId
        {
            PieceIdZero = 0,
            FriendHandPawn = PieceIdZero + 1,
            EnemyHandPawn = 20,

            FriendHandLance = 39,
            EnemyHandLance = 44,
            FriendHandKnight = 49,
            EnemyHandKnight = 54,
            FriendHandSilver = 59,
            EnemyHandSilver = 64,
            FriendHandGold = 69,
            EnemyHandGold = 74,
            FriendHandBishop = 79,
            EnemyHandBishop = 82,
            FriendHandRook = 85,
            EnemyHandRook = 88,
            FriendEnemyHandEnd = 90,

            FriendPawn = FriendEnemyHandEnd,
            EnemyPawn = FriendPawn + 81,
            FriendLance = EnemyPawn + 81,
            EnemyLance = FriendLance + 81,
            FriendKnight = EnemyLance + 81,
            EnemyKnight = FriendKnight + 81,
            FriendSilver = EnemyKnight + 81,
            EnemySilver = FriendSilver + 81,
            FriendGold = EnemySilver + 81,
            EnemyGold = FriendGold + 81,
            FriendBishop = EnemyGold + 81,
            EnemyBishop = FriendBishop + 81,
            FriendHorse = EnemyBishop + 81,
            EnemyHorse = FriendHorse + 81,
            FriendRook = EnemyHorse + 81,
            EnemyRook = FriendRook + 81,
            FriendDragon = EnemyRook + 81,
            EnemyDragon = FriendDragon + 81,
            FriendEnemyEnd = EnemyDragon + 81,

            FriendKing = FriendEnemyEnd,
            EnemyKing = FriendKing + _boardSize,
            FriendeEnemyEnd2 = EnemyKing + _boardSize,
        };

        /// <summary>
        /// 盤上の駒のPieceIdのオフセット。
        /// [Piece][先手視点・後手視点]でアクセスする。
        /// </summary>
        private static PieceId[] BoardPieceIds = new PieceId[]{
            PieceId.PieceIdZero,
            PieceId.FriendPawn,
            PieceId.FriendLance,
            PieceId.FriendKnight,
            PieceId.FriendSilver,
            PieceId.FriendGold,
            PieceId.FriendBishop,
            PieceId.FriendRook,
            PieceId.PieceIdZero,
            PieceId.FriendGold,
            PieceId.FriendGold,
            PieceId.FriendGold,
            PieceId.FriendGold,
            PieceId.FriendHorse,
            PieceId.FriendDragon,
            PieceId.EnemyPawn,
            PieceId.EnemyLance,
            PieceId.EnemyKnight,
            PieceId.EnemySilver,
            PieceId.EnemyGold,
            PieceId.EnemyBishop,
            PieceId.EnemyRook,
            PieceId.PieceIdZero,
            PieceId.EnemyGold,
            PieceId.EnemyGold,
            PieceId.EnemyGold,
            PieceId.EnemyGold,
            PieceId.EnemyHorse,
            PieceId.EnemyDragon,
        };

        /// <summary>
        /// 持ち駒のPieceIdのオフセット。
        /// [Piece][先手視点・後手視点]でアクセスする。
        /// </summary>
        private static PieceId[] HandPieceIds = new PieceId[] {
            PieceId.PieceIdZero,
            PieceId.FriendHandPawn,
            PieceId.FriendHandLance,
            PieceId.FriendHandKnight,
            PieceId.FriendHandSilver,
            PieceId.FriendHandGold,
            PieceId.FriendHandBishop,
            PieceId.FriendHandRook,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.EnemyHandPawn,
            PieceId.EnemyHandLance,
            PieceId.EnemyHandKnight,
            PieceId.EnemyHandSilver,
            PieceId.EnemyHandGold,
            PieceId.EnemyHandBishop,
            PieceId.EnemyHandRook,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
            PieceId.PieceIdZero,
        };
		
        
        public const int ZeroValue = 0;
        public const int MateValue = 32000;
        public const int InfiniteValue = 32001;
        public const int InvalidValue = 32002;
        public const int MaxPlay = 128;
        public const int MateInMaxPlayValue = MateValue - MaxPlay;
        public const int MatedInMaxPlayValue = -MateValue + MaxPlay;
        public const int DrawValue = -1;

        public const int PawnValue = 90;
        public const int LanceValue = 315;
        public const int KnightValue = 405;
        public const int SilverValue = 495;
        public const int GoldValue = 540;
        public const int BishopValue = 855;
        public const int RookValue = 990;
        public const int ProPawnValue = 540;
        public const int ProLanceValue = 540;
        public const int ProKnightValue = 540;
        public const int ProSilverValue = 540;
        public const int HorseValue = 945;
        public const int DragonValue = 1395;
        public const int KingValue = 15000;
        private const int HalfDimentions = 256;
        private const int WeightScaleBits = 6;
        private const int FVScale = 16;
        private const int _MM_PERM_BADC = 0x4E;
        private const int _MM_PERM_CDAB = 0xB1;

        private static int[] MaterialValues =
        {
	        ZeroValue,
	        PawnValue,
	        LanceValue,
	        KnightValue,
	        SilverValue,
	        GoldValue,
	        BishopValue,
	        RookValue,
	        KingValue,
	        ProPawnValue,
	        ProLanceValue,
	        ProKnightValue,
	        ProSilverValue,
	        HorseValue,
	        DragonValue,
	        -PawnValue,
	        -LanceValue,
	        -KnightValue,
	        -SilverValue,
	        -GoldValue,
	        -BishopValue,
	        -RookValue,
	        -KingValue,
	        -ProPawnValue,
	        -ProLanceValue,
	        -ProKnightValue,
	        -ProSilverValue,
	        -HorseValue,
	        -DragonValue,
	        InvalidValue,
        };
		private const int HalfDimensions = 256;
		private short[] featureTransformerBiases = new short[HalfDimensions];
		private short[] featureTransformerWeights = new short[HalfDimensions * 125388];
		private int[] firstBiases = new int[32];
		private sbyte[] firstWeights = new sbyte[32 * HalfDimensions * 2];
		private int[] secondBiases = new int[32];
		private sbyte[] secondWeights = new sbyte[32 * 32];
		private int[] thirdBiases = new int[1];
		private sbyte[] thirdWeights = new sbyte[1 * 32];
		
		public short[] Z1Black { get; set; }
		public short[] Z1White { get; set; }
		public void Load()
		{
			var evalFilePath = "../../../Models/nn.bin";
			
			using (var reader = new BinaryReader(File.Open(evalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                var version = reader.ReadUInt32();
                
                var hashValue = reader.ReadUInt32();

                var size = reader.ReadUInt32();
                var architecture = reader.ReadBytes((int)size);

                // 入力層と隠れ層第1層の間のネットワークパラメーター
                var featureTransformerHeader = reader.ReadUInt32();
                
                for (int i = 0; i < featureTransformerBiases.Length; ++i)
                {
                    featureTransformerBiases[i] = reader.ReadInt16();
                }
                for (int i = 0; i < featureTransformerWeights.Length; ++i)
                {
                    featureTransformerWeights[i] = reader.ReadInt16();
                }

                // 隠れ層第1層と隠れ層第2層の間のネットワークパラメーター
                var networkHeader = reader.ReadUInt32();
                
                for (int i = 0; i < firstBiases.Length; ++i)
                {
                    firstBiases[i] = reader.ReadInt32();
                }
                for (int i = 0; i < firstWeights.Length; ++i)
                {
                    firstWeights[i] = reader.ReadSByte();
                }

                // 隠れ層第2層と隠れ層第3層の間のネットワークパラメーター
                for (int i = 0; i < secondBiases.Length; ++i)
                {
                    secondBiases[i] = reader.ReadInt32();
                }
                for (int i = 0; i < secondWeights.Length; ++i)
                {
                    secondWeights[i] = reader.ReadSByte();
                }
                // 隠れ層第3層と出力層の間のネットワークパラメーター
                for (int i = 0; i < thirdBiases.Length; ++i)
                {
                    thirdBiases[i] = reader.ReadInt32();
                }
                for (int i = 0; i < thirdWeights.Length; ++i)
                {
                    thirdWeights[i] = reader.ReadSByte();
                }
                
            }

            _logger.Log("info string Loaded an eval file...");
			
		}

		public int Evaluate()
		{
			return 0;
		}

		/// <summary>
		/// 全計算を行う
		/// </summary>
		public void UpdateAccumulatorFully()
		{
			// 全計算
			Z1Black = new short[HalfDimensions];
			Z1White = new short[HalfDimensions];

			// バイアスベクトルをコピーする
			Array.Copy(featureTransformerBiases, Z1Black, HalfDimensions);
			Array.Copy(featureTransformerBiases, Z1White, HalfDimensions);
			
			// 盤上の駒
			for (int file = 0; file < _boardSize; ++file)
			{
				for (int rank = 0; rank < _boardSize; ++rank)
				{
					// if (_position.Board[file, rank] == Piece.Empty
					//     || _position.Board[file, rank] == Piece.B_King
					//     || _position.Board[file, rank] == Piece.W_King)
					// {
					// 	continue;
					// }
					//
					// Add(_position,
					// 	MakeBoardPieceId(_position.Board[file, rank], file, rank),
					// 	MakeBoardPieceId(_position.Board[file, rank].AsOpponentPiece(), 8 - file, 8 - rank));
				}
			}

			// 持ち駒
			for (var handPiece = Piece.Empty; handPiece < (Piece)30; ++handPiece)
			{
				//for (int numHandPieces = 1; numHandPieces <= _position.HandPieces[(int)handPiece]; ++numHandPieces)
				{
					// Add(_position,
					// 	MakeHandPieceId(handPiece, numHandPieces),
					// 	MakeHandPieceId(handPiece.AsOpponentPiece(), numHandPieces));
				}
			}
		}
		
		/// <summary>
        /// 特徴ベクトルをアフィン変換し保持してあるベクトルに、重み行列の列を足す。
        /// </summary>
        private void Add(Position position, int pieceIdFromBlack, int pieceIdFromWhite)
        {

        }

        /// <summary>
        /// 特徴ベクトルをアフィン変換し保持してあるベクトルから、重み行列の列を引く。
        /// </summary>
        private void Subtract(Position position, int pieceIdFromBlack, int pieceIdFromWhite)
        {
	        
        }
		
		/// <summary>
		/// 盤上の駒のPieceIdを計算する。
		/// </summary>
		private static int MakeBoardPieceId(Piece piece, int file, int rank)
		{
			int square = file * _boardSize + rank;
			PieceId offset = BoardPieceIds[(int)piece];
			return (int)offset + square;
		}

		/// <summary>
		/// 持ち駒のPieceIdを計算する
		/// </summary>
		private static int MakeHandPieceId(Piece piece, int numHandPieces)
		{
			PieceId offset = HandPieceIds[(int)piece];
			// 1枚目の持ち駒のIDは0から始まるので、1引く
			return (int)offset + numHandPieces - 1;
		}
		
		/// <summary>
		/// KP特徴量インデックスを計算する。
		/// </summary>
		private static int MakeKPIndex(int kingFile, int kingRank, int pieceId)
		{
			return (kingFile * _boardSize + kingRank) * (int)PieceId.FriendEnemyEnd + pieceId;
		}
		
		protected override UniTask<string> Usi()
		{
			_logger.Log("id name UniShogi");
			_logger.Log("id author Tsubasa Hizono");
			return UniTask.FromResult("usiok");
		}

		protected override UniTask<string> IsReady()
		{
			Load();
			return UniTask.FromResult("readyok");
		}

		protected override UniTask<string> UsiNewGame()
		{
			return UniTask.FromResult("usinewgame");
		}

		/// <summary>
		/// 局面を受け取る
		/// </summary>
		protected override UniTask<string> Position(string[] command)
		{
			_position.Set(command[1]);
			return UniTask.FromResult(string.Empty);
		}

		protected override UniTask<string> Go()
		{
			// 局面から合法手を一つ選びそれを返す
			var moves = Movegen.GenerateMoves(_position);
			var move = moves[_random.Next(moves.Count)];
			return UniTask.FromResult($"bestmove {move.ToUsi()}");
		}

		protected override UniTask<string> Stop()
		{
			return UniTask.FromResult("stop");
		}

		protected override UniTask<string> Quit()
		{
			return UniTask.FromResult("quit");
		}
	}
}