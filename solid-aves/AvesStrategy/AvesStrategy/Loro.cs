///////////////////////////////////////////////////////////
//  Loro.cs
//  Implementation of the Class Loro
//  Generated by Enterprise Architect
//  Created on:      20-may.-2017 12:36:43 p. m.
//  Original author: user
///////////////////////////////////////////////////////////




public class Loro : AveVoladora {

	private string canto;

    private Audio _recordstrategy;

    public Loro(){

	}

	public string getCanto(){
		return this.canto;
    }

    public void SetRecordStrategy(Audio recordstrategy)
    {
        this._recordstrategy = recordstrategy;
    }

    public void record()
    {
        _recordstrategy.record();
    }

}//end Loro
