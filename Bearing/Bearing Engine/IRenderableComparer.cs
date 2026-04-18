using System.Collections.Generic;

namespace Bearing;

public class IRenderableComparer : IComparer<IRenderable>
{
	private int CalculateScore(IRenderable r)
	{
		int result = 0;

		if (r.isTransparent) result++;

		return result;
	}

    public int Compare(IRenderable? x, IRenderable? y)
    {
    	if (x is null || y is null)
    		throw new ArgumentNullException();

    	IRenderable r1 = (IRenderable)x;
    	IRenderable r2 = (IRenderable)y;

    	int score1 = CalculateScore(r1);
    	int score2 = CalculateScore(r2);

    	int result = Math.Clamp(score1-score2, -1, 1);

    	return result;
    }
}