///////////////////////////////////////////////////////////
//  Loro.cs
//  Implementation of the Class Loro
//  Generated by Enterprise Architect
//  Created on:      20-may.-2017 12:36:43 p. m.
//  Original author: user
///////////////////////////////////////////////////////////




using System;

public class Loro : AveVoladora {

	private string _canto { get; set; }

	public Loro()
    {
	}


    public string record(string tipo)
    {
        if (tipo == "mp3")
        { return "Estoy grabando a un loro en formato mp3"; }
        else if (tipo == "wma")
        { return "Estoy grabando a un loro en formato wma"; }
        else
        { return "Formato de grabaci�n desconocido"; }
    }
}//end Loro
  