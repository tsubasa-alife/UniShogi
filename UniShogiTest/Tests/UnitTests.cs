using UniShogi;

[TestClass]
public class UnitTests
{
	[TestMethod]
	public void TestShogiLib()
	{
		var pos = new Position("lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1"); // 平手

		Assert.AreEqual(pos.Sfen(),"lnsgkgsnl/1r5b1/ppppppppp/9/9/9/PPPPPPPPP/1B5R1/LNSGKGSNL b - 1");

		pos.DoMove(Usi.ParseMove("2g2f"));

		Assert.AreEqual(pos.Player == Color.White, true);

		pos.UndoMove();
			
		Assert.AreEqual(pos.InCheck(), false);
		Assert.AreEqual(pos.IsMated(), false);
		Assert.AreEqual(pos.IsLegalMove(Usi.ParseMove("7g7f")), true);
		Assert.AreEqual(pos.CheckRepetition() == Repetition.None, true);
		Assert.AreEqual(pos.CanDeclareWin(),false);

		Assert.AreEqual(Movegen.GenerateMoves(pos).Count, 31);
	}
}