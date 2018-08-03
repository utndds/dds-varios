///////////////////////////////////////////////////////////
//  AveVoladora.cs
//  Implementation of the Class AveVoladora
//  Generated by Enterprise Architect
//  Created on:      20-may.-2017 12:36:42 p. m.
//  Original author: user
///////////////////////////////////////////////////////////




public class AveVoladora : Ave {

    // Property altitud

	private int _altitud { get; set; }

    public void setAltitud (int value)
    {
        _altitud = value;
    }

    public int getAltitud()
    {
        return _altitud;
    }

    // Property horas voladas

    private double _horasVoladas { get; set; }

    public double getHorasVoladas()
    {
        return _horasVoladas;
    }

    // Constructor

    public AveVoladora()
    {
	}


    // Methods

	/// 
	/// <param name="horas"></param>
	public void volar(double horas)
    {
        this._horasVoladas = this._horasVoladas + horas;
	}

}//end AveVoladora