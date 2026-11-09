Option Strict Off
Option Explicit On

Imports atcData
Imports atcUtility
Imports MapWinUtility

Public Module modComputeWea
    Friend MetComputeLatitudeMax As Double = 66.5
    Friend MetComputeLatitudeMin As Double = -66.5
    Friend Const DegreesToRadians As Double = 0.01745329252
    Private X1() As Double = {0, 10.00028, 41.0003, 69.22113, 100.5259, 130.8852, 161.2853,
                          191.7178, 222.1775, 253.66, 281.1629, 309.6838, 341.221}
    Private c(,) As Double = {
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 4.0, 2.0, -1.5, -3.0, -2.0, 1.0, 3.0, 2.5, 1.0, 1.0, 2.0, 1.0},
      {0, 3.0, 4.0, 0.0, -3.0, -2.5, 0.0, 2.0, 3.0, 2.0, 1.5, 2.0, 1.0},
      {0, 0.0, 3.5, 1.5, -1.0, -2.0, -1.0, 1.5, 3.0, 3.0, 1.5, 2.0, 1.0},
      {0, -2.0, 2.5, 3.5, 0.0, -2.0, -1.0, 0.5, 3.0, 3.0, 2.0, 2.0, 1.0},
      {0, -4.0, 0.5, 3.0, 1.0, -0.5, -1.0, 0.0, 2.0, 2.5, 2.5, 2.0, 1.0},
      {0, -5.0, -1.5, 2.0, 3.0, 0.5, -1.0, -0.5, 1.0, 2.5, 2.5, 2.0, 1.0},
      {0, -5.0, -3.5, 1.0, 3.0, 1.5, 0.0, -0.5, 1.0, 2.0, 2.0, 2.0, 1.0},
      {0, -4.0, -4.5, -1.0, 2.5, 3.0, 1.0, 0.0, 0.0, 1.5, 2.0, 2.0, 1.0},
      {0, -2.0, -4.0, -3.0, 1.0, 3.0, 2.0, 0.5, 0.0, 1.5, 2.0, 1.0, 1.0},
      {0, 0.0, -3.5, -4.0, -0.5, 3.0, 3.0, 1.5, 1.0, 1.0, 2.0, 1.0, 1.0}}
    Private XLax(,) As Double = {
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9}, {-9, -9, -9, -9, -9, -9, -9},
      {-9, -9, -9, -9, -9, -9, -9},
      {-9, 616.17, -147.83, -27.17, -3.17, 11.84, 2.02},
      {-9, 609.97, -154.71, -27.49, -2.97, 12.04, 1.3},
      {-9, 603.69, -161.55, -27.69, -2.78, 12.22, 0.64},
      {-9, 597.29, -168.33, -27.78, -2.6, 12.38, 0.02},
      {-9, 590.81, -175.05, -27.74, -2.43, 12.53, -0.56},
      {-9, 584.21, -181.72, -27.57, -2.28, 12.67, -1.1},
      {-9, 577.53, -188.34, -27.29, -2.14, 12.8, -1.6},
      {-9, 570.73, -194.91, -26.89, -2.02, 12.92, -2.05},
      {-9, 563.85, -201.42, -26.37, -1.91, 13.03, -2.45},
      {-9, 556.85, -207.29, -25.72, -1.81, 13.13, -2.8},
      {-9, 549.77, -214.29, -24.96, -1.72, 13.22, -3.1},
      {-9, 542.57, -220.65, -24.07, -1.64, 13.3, -3.35},
      {-9, 535.3, -226.96, -23.07, -1.59, 13.36, -3.58},
      {-9, 527.9, -233.22, -21.95, -1.55, 13.4, -3.77},
      {-9, 520.44, -239.43, -20.7, -1.52, 13.42, -3.92},
      {-9, 512.84, -245.59, -19.33, -1.51, 13.42, -4.03},
      {-9, 505.19, -251.69, -17.83, -1.51, 13.41, -4.1},
      {-9, 497.4, -257.74, -16.22, -1.52, 13.39, -4.13},
      {-9, 489.52, -263.74, -14.49, -1.54, 13.36, -4.12},
      {-9, 481.53, -269.7, -12.63, -1.57, 13.32, -4.07},
      {-9, 473.45, -275.6, -10.65, -1.63, 13.27, -3.98},
      {-9, 465.27, -281.45, -8.55, -1.71, 13.21, -3.85},
      {-9, 456.99, -287.25, -6.33, -1.8, 13.14, -3.68},
      {-9, 448.61, -292.99, -3.98, -1.9, 13.07, -3.47},
      {-9, 440.14, -298.68, -1.51, -2.01, 13.0#, -3.3},
      {-9, 431.55, -304.32, 1.08, -2.13, 12.92, -3.17},
      {-9, 431.55, -304.32, 1.08, -2.13, 12.92, -3.17}}
    Private Triang(,) As Double = {
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.01},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0.1, 0.11},
      {0, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0.08, 0.09, 0.45, 0.55},
      {0, 0, 0, 0, 0, 0.01, 0.01, 0.06, 0.07, 0.28, 0.36, 1.2, 1.65},
      {0, 0, 0, 0.01, 0.01, 0.04, 0.05, 0.15, 0.21, 0.56, 0.84, 2.1, 3.3},
      {0, 0.01, 0.01, 0.02, 0.03, 0.06, 0.1, 0.2, 0.35, 0.7, 1.26, 2.52, 4.62},
      {0, 0, 0.01, 0.01, 0.03, 0.04, 0.1, 0.15, 0.35, 0.56, 1.26, 2.1, 4.62},
      {0, 0, 0, 0, 0.01, 0.01, 0.05, 0.06, 0.21, 0.28, 0.84, 1.2, 3.3},
      {0, 0, 0, 0, 0, 0, 0.01, 0.01, 0.07, 0.08, 0.36, 0.45, 1.65},
      {0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0.09, 0.1, 0.55},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01, 0.01, 0.11},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.01},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
      {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}}
    Private Sums() As Double = {0, 0.01, 0.02, 0.04, 0.08, 0.16, 0.32, 0.64, 1.28, 2.56, 5.12, 10.24, 20.48}

    ''' <summary>
    ''' Disaggregate daily SOLAR or PET to hourly
    ''' </summary>
    ''' <param name="aInTs">input timeseries to be disaggregated</param>
    ''' <param name="aDataSource"></param>
    ''' <param name="aDisOpt">1 does Solar, DisOpt = 2 does PET</param>
    ''' <param name="aLatDeg">Latitude, in degrees</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    Public Function DisSolPet(ByVal aInTs As atcTimeseries, ByVal aDataSource As atcTimeseriesSource, ByVal aDisOpt As Integer, ByVal aLatDeg As Double) As atcTimeseries
        Dim lHrPos, i, j, retcod As Integer
        Dim lDate(5) As Integer
        Dim lDisTs As New atcTimeseries(aDataSource)
        Dim lPoint As Boolean = aInTs.Attributes.GetValue("point", False)
        Dim lHrVals(24) As Double
        Dim lNaN As Double = GetNaN()

        CopyBaseAttributes(aInTs, lDisTs)
        lDisTs.Attributes.SetValue("Scenario", "COMPUTED")
        lDisTs.SetInterval(atcTimeUnit.TUHour, 1)
        If aDisOpt = 1 Then 'solar disaggregation
            lDisTs.Attributes.SetValue("Constituent", "SOLR")
            lDisTs.Attributes.SetValue("TSTYPE", "SOLR")
            lDisTs.Attributes.SetValue("Description", "Hourly Solar Radiation (Langleys) disaggregated from Daily")
            lDisTs.Attributes.AddHistory("Disaggregated Solar Radiation - inputs: SRAD, Latitude")
            lDisTs.Attributes.Add("SRAD", aInTs.ToString)
        Else 'ET disaggregation
            lDisTs.Attributes.SetValue("Constituent", "EVAP")
            lDisTs.Attributes.SetValue("TSTYPE", "EVAP")
            lDisTs.Attributes.SetValue("Description", "Hourly Evapotranspiration disaggregated from Daily")
            lDisTs.Attributes.AddHistory("Disaggregated Evapotranspiration - inputs: DEVT, Latitude")
            lDisTs.Attributes.Add("DEVT", aInTs.ToString)
        End If
        lDisTs.Attributes.Add("Latitude", aLatDeg)

        lDisTs.Dates = DisaggDates(aInTs, aDataSource)
        lDisTs.numValues = lDisTs.Dates.numValues
        If lDisTs.numValues < aInTs.numValues * 24 Then
            Logger.Dbg("NumValueProblem " & lDisTs.numValues & " " & aInTs.numValues)
            lDisTs.numValues = aInTs.numValues * 24 'kludge!
        End If

        Dim lOutTs(lDisTs.numValues) As Double
        lHrPos = 0
        For i = 1 To aInTs.numValues
            If Not Double.IsNaN(aInTs.Value(i)) Then
                If lPoint Then
                    Call J2Date(aInTs.Dates.Value(i), lDate)
                Else
                    Call J2Date(aInTs.Dates.Value(i - 1), lDate)
                End If
                If aDisOpt = 1 Then 'solar
                    Call RADDST(aLatDeg, lDate(1), lDate(2), aInTs.Value(i), lHrVals, retcod)
                ElseIf aDisOpt = 2 Then  'pet
                    Call PETDST(aLatDeg, lDate(1), lDate(2), aInTs.Value(i), lHrVals, retcod)
                End If
                For j = 1 To 24
                    lOutTs(lHrPos + j) = lHrVals(j)
                Next j
            Else
                For j = 1 To 24
                    lOutTs(lHrPos + j) = lNaN
                Next j
            End If
            'increment to next 24 hour period
            lHrPos = lHrPos + 24
        Next i
        Array.Copy(lOutTs, 1, lDisTs.Values, 1, lDisTs.numValues)
        Return lDisTs

    End Function

    Private Function DisaggDates(ByVal aInTS As atcTimeseries, ByVal aDataSource As atcTimeseriesSource) As atcTimeseries
        'build new date timeseries class for hourly TSer based on daily TSer (aInTS)

        Dim lDates As New atcTimeseries(aDataSource)
        'lDates.numValues = aInTS.numValues * 24
        'lDates.ValuesNeedToBeRead = True
        lDates.Values = NewDates(aInTS, atcTimeUnit.TUHour, 1)
        Return lDates

        ''NOTE: Only valid for constant interval timeseries
        'Dim lPoint As Boolean = aInTS.Attributes.GetValue("point", False)

        'If lPoint Then
        '    'lDates.Value(ip) = GetNaN
        '    Return Nothing
        'Else
        '    Dim lJDay As Double
        '    Dim lDates As New atcTimeseries(aDataSource)
        '    lDates.numValues = aInTS.numValues * 24
        '    lJDay = aInTS.Attributes.GetValue("SJDAY")
        '    Dim ip As Integer = 0
        '    lDates.Value(ip) = lJDay
        '    'lJDay += lHrInc
        '    For i As Integer = 0 To aInTS.numValues - 1
        '        For j As Integer = 1 To 24
        '            ip += 1
        '            lDates.Value(ip) = aInTS.Dates.Value(i) + j * JulianHour
        '        Next
        '    Next
        '    Return lDates
        'End If
        ''For i As Integer = 1 To lDates.numValues
        ''    lDates.Value(i) = lJDay
        ''    lJDay += lHrInc
        ''Next i
    End Function

    Public Sub PETDST(ByVal aLatDeg As Double, ByVal aMonth As Integer, ByVal aDay As Integer, ByVal aDayPet As Double, ByRef aHrPet() As Double, ByRef aRetCod As Integer)

        'Distributes daily PET to hourly values,
        'based on a method used to disaggregate solar radiation
        'in HSP (Hydrocomp, 1976) using latitude, month, day,
        'and daily PET.

        'aLatDeg - latitude(degrees)
        'aMONTH  - month of the year (1-12)
        'aDAY    - day of the month (1-31)
        'aDAYPET - input daily PET (inches)
        'aHRPET  - output array of hourly PET (inches)
        'aRETCOD - return code (0 = ok, -1 = bad latitude)

        Dim IK As Integer
        Dim TR3, TRise, CRAD, DTR2, Delt, CS, AD, JulDay, RK, LatRdn, Phi, SS, X2, SunR, DTR4, SL, TR2, TR4 As Double
        Dim CURVE(24) As Double

        'julian date
        JulDay = 30.5 * (aMonth - 1) + aDay

        'check latitude
        If aLatDeg < MetComputeLatitudeMin OrElse aLatDeg > MetComputeLatitudeMax Then 'invalid latitude, return
            aRetCod = -1
        Else 'latitude ok
            'convert to radians
            LatRdn = aLatDeg * DegreesToRadians

            Phi = LatRdn
            AD = 0.40928 * Math.Cos(0.0172141 * (172.0# - JulDay))
            SS = Math.Sin(Phi) * Math.Sin(AD)
            CS = Math.Cos(Phi) * Math.Cos(AD)
            X2 = -SS / CS
            Delt = 7.6394 * (1.5708 - Math.Atan(X2 / Math.Sqrt(1.0# - X2 ^ 2)))
            SunR = 12.0# - Delt / 2.0#

            'develop hourly distribution given sunrise,
            'sunset and length of day (DELT)
            DTR2 = Delt / 2.0#
            DTR4 = Delt / 4.0#
            CRAD = 0.66666667 / DTR2
            SL = CRAD / DTR4
            TRise = SunR
            TR2 = TRise + DTR4
            TR3 = TR2 + DTR2
            TR4 = TR3 + DTR4

            'calculate hourly distribution curve
            For IK = 1 To 24
                RK = IK
                If RK > TRise Then
                    If RK > TR2 Then
                        If RK > TR3 Then
                            If RK > TR4 Then
                                CURVE(IK) = 0.0#
                                aHrPet(IK) = CURVE(IK)
                            Else
                                CURVE(IK) = (CRAD - (RK - TR3) * SL)
                                aHrPet(IK) = CURVE(IK) * aDayPet
                            End If
                        Else
                            CURVE(IK) = CRAD
                            aHrPet(IK) = CURVE(IK) * aDayPet
                        End If
                    Else
                        CURVE(IK) = (RK - TRise) * SL
                        aHrPet(IK) = CURVE(IK) * aDayPet
                    End If
                Else
                    CURVE(IK) = 0.0#
                    aHrPet(IK) = CURVE(IK)
                End If
                If aHrPet(IK) > 40 Then
                    Logger.Dbg("Bad Hourly Value " & aHrPet(IK))
                End If

            Next IK
            aRetCod = 0
        End If

    End Sub

    Private Sub RADDST(ByVal aLatDeg As Double, ByVal aMonth As Integer, ByVal aDay As Integer, ByVal aDayRad As Double, ByRef aHrRad() As Double, ByRef aRetCod As Integer)
        'Distributes daily solar radiation to hourly
        'values, based on a method used in HSP (Hydrocomp, 1976).
        'It uses the latitude, month, day, and daily radiation.

        'aLatDeg - latitude(degrees)
        'aMONTH  - month of the year (1-12)
        'aDAY    - day of the month (1-31)
        'aDAYRAD - input daily radiation (langleys)
        'aHRRAD  - output array of hourly radiation (langleys)
        'aRETCOD - return code (0 = ok, -1 = bad latitude)

        Dim IK As Integer
        Dim TR3, TRise, CRAD, DTR2, Delt, CS, AD, JulDay, RK, LatRdn, Phi, SS, X2, SunR, DTR4, SL, TR2, TR4 As Double

        'julian date
        JulDay = 30.5 * (aMonth - 1) + aDay

        'check latitude
        If aLatDeg < MetComputeLatitudeMin OrElse aLatDeg > MetComputeLatitudeMax Then 'invalid latitude, return
            aRetCod = -1
        Else 'latitude ok
            'convert to radians
            LatRdn = aLatDeg * DegreesToRadians

            Phi = LatRdn
            AD = 0.40928 * System.Math.Cos(0.0172141 * (172.0# - JulDay))
            SS = Math.Sin(Phi) * Math.Sin(AD)
            CS = Math.Cos(Phi) * Math.Cos(AD)
            X2 = -SS / CS
            Delt = 7.6394 * (1.5708 - Math.Atan(X2 / Math.Sqrt(1.0# - X2 ^ 2)))
            SunR = 12.0# - Delt / 2.0#

            'develop hourly distribution given sunrise,
            'sunset and length of day (DELT)
            DTR2 = Delt / 2.0#
            DTR4 = Delt / 4.0#
            CRAD = 0.66666667 / DTR2
            SL = CRAD / DTR4
            TRise = SunR
            TR2 = TRise + DTR4
            TR3 = TR2 + DTR2
            TR4 = TR3 + DTR4

            'hourly loop
            For IK = 1 To 24
                RK = IK
                If RK > TRise Then
                    If RK > TR2 Then
                        If RK > TR3 Then
                            If RK > TR4 Then
                                aHrRad(IK) = 0.0#
                            Else
                                aHrRad(IK) = (CRAD - (RK - TR3) * SL) * aDayRad
                            End If
                        Else
                            aHrRad(IK) = CRAD * aDayRad
                        End If
                    Else
                        aHrRad(IK) = (RK - TRise) * SL * aDayRad
                    End If
                Else
                    aHrRad(IK) = 0.0#
                End If
            Next IK
            aRetCod = 0
        End If

    End Sub

    ''' <summary>compute Penman - PET</summary>
    ''' <param name="aTMinTS">Min Air Temperature - daily</param>
    ''' <param name="aTMaxTS">Max Air Temperature - daily</param>
    ''' <param name="aSRadTS">Solar Radiation</param>
    ''' <param name="aDewPTS">Dewpoint Temperature</param>
    ''' <param name="aWindTS">Wind Movement</param>
    ''' <param name="aSource"></param>
    ''' <returns>Pan Evaporation timeseries - daily timestep</returns>
    ''' <remarks>The computations are based on the Penman(1948) formula and the method of Kohler, Nordensen, and Fox (1955).</remarks>
    ''' 
    Public Function PanEvaporationTimeseriesComputedByPenman(ByVal aTMinTS As atcTimeseries, ByVal aTMaxTS As atcTimeseries, ByVal aSRadTS As atcTimeseries, ByVal aDewPTS As atcTimeseries, ByVal aWindTS As atcTimeseries, ByVal aSource As atcTimeseriesSource) As atcTimeseries
        Dim lPanEvapTimeSeries As New atcTimeseries(aSource)
        CopyBaseAttributes(aTMinTS, lPanEvapTimeSeries)
        lPanEvapTimeSeries.Attributes.SetValue("Constituent", "PPAN")
        lPanEvapTimeSeries.Attributes.SetValue("TSTYPE", "PPAN")
        lPanEvapTimeSeries.Attributes.SetValue("Scenario", "COMPUTED")
        lPanEvapTimeSeries.Attributes.SetValue("Description", "Daily Pan Evaporation (in) computed using Penman algorithm")
        lPanEvapTimeSeries.Attributes.AddHistory("Computed Daily Pan Evaporation using Penman - inputs: TMIN, TMAX, SRAD, DEWP, WIND")
        lPanEvapTimeSeries.Attributes.Add("TMIN", aTMinTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("TMAX", aTMaxTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("SRAD", aSRadTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("DEWP", aDewPTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("WIND", aWindTS.ToString)
        lPanEvapTimeSeries.Dates = aTMinTS.Dates
        lPanEvapTimeSeries.numValues = aTMinTS.numValues

        Dim lMissingValue(5) As Double
        lMissingValue(1) = aTMinTS.Attributes.GetValue("TSFILL", -999)
        lMissingValue(2) = aTMaxTS.Attributes.GetValue("TSFILL", -999)
        lMissingValue(3) = aSRadTS.Attributes.GetValue("TSFILL", -999)
        lMissingValue(4) = aDewPTS.Attributes.GetValue("TSFILL", -999)
        lMissingValue(5) = aWindTS.Attributes.GetValue("TSFILL", -999)

        Dim lPanEvapValues(aTMinTS.numValues) As Double
        For lValueIndex As Integer = 1 To lPanEvapTimeSeries.numValues
            If Math.Abs(aTMinTS.Value(lValueIndex) - lMissingValue(1)) < 0.000001 OrElse
               Math.Abs(aTMaxTS.Value(lValueIndex) - lMissingValue(2)) < 0.000001 OrElse
               Math.Abs(aSRadTS.Value(lValueIndex) - lMissingValue(3)) < 0.000001 OrElse
               Math.Abs(aDewPTS.Value(lValueIndex) - lMissingValue(4)) < 0.000001 OrElse
               Math.Abs(aWindTS.Value(lValueIndex) - lMissingValue(5)) < 0.000001 Then
                'missing source data
                lPanEvapValues(lValueIndex) = lMissingValue(1)
            Else 'compute pet
                lPanEvapValues(lValueIndex) = PanEvaporationValueComputedByPenman(aTMinTS.Value(lValueIndex), aTMaxTS.Value(lValueIndex), aDewPTS.Value(lValueIndex), aWindTS.Value(lValueIndex), aSRadTS.Value(lValueIndex))
            End If
        Next lValueIndex

        Array.Copy(lPanEvapValues, 1, lPanEvapTimeSeries.Values, 1, lPanEvapTimeSeries.numValues)
        Return lPanEvapTimeSeries
    End Function
    ''' <summary>
    ''' Compute daily pan evaporation (inches)
    ''' </summary>
    ''' <param name="aMinTmp">daily minimum air temperature (degF)</param>
    ''' <param name="aMaxTmp">daily maximum air temperature (degF)</param>
    ''' <param name="aDewTmp">dewpoint temperature (degF)</param>
    ''' <param name="aWindSp">wind movement (miles/day)</param>
    ''' <param name="aSolRad">solar radiation (langleys/day)</param>
    ''' <returns>pan evaporation (inches/day)</returns>
    ''' <remarks>based on the Penman(1948) formula and the method of Kohler, Nordensen, and Fox (1955).</remarks>
    Private Function PanEvaporationValueComputedByPenman(ByVal aMinTmp As Double, ByVal aMaxTmp As Double, ByVal aDewTmp As Double, ByVal aWindSp As Double, ByVal aSolRad As Double) As Double
        'compute average daily air temperature
        Dim lAirTmp As Double = (aMinTmp + aMaxTmp) / 2.0#

        'net radiation exchange * delta
        If aSolRad <= 0.0# Then aSolRad = 0.00001
        Dim lQNDelt As Double = Math.Exp((lAirTmp - 212.0#) * (0.1024 - 0.01066 * Math.Log(aSolRad))) - 0.0001

        'Vapor pressure deficit between surface and
        'dewpoint temps(Es-Ea) IN of Hg
        Dim lEsMiEa As Double = (6413252.0# * System.Math.Exp(-7482.6 / (lAirTmp + 398.36))) - (6413252.0# * Math.Exp(-7482.6 / (aDewTmp + 398.36)))

        'pan evaporation assuming air temp equals water surface temp.

        'when vapor pressure deficit turns negative it is set equal to zero
        If lEsMiEa < 0.0# Then
            lEsMiEa = 0.0#
        End If

        'pan evap * GAMMA, GAMMA = 0.0105 inch Hg/F
        Dim lEaGama As Double = 0.0105 * (lEsMiEa ^ 0.88) * (0.37 + 0.0041 * aWindSp)

        'Delta = slope of saturation vapor pressure curve at air temperature
        Dim lDelta As Double = 47987800000.0# * Math.Exp(-7482.6 / (lAirTmp + 398.36)) / ((lAirTmp + 398.36) ^ 2)

        'pan evaporation rate in inches per day
        Dim lPanEvap As Double = (lQNDelt + lEaGama) / (lDelta + 0.0105)

        'when the estimated pan evaporation is negative
        'the value is set to zero
        If lPanEvap < 0.0# Then
            lPanEvap = 0.0#
        End If

        Return lPanEvap
    End Function
    ''' <summary>compute Hamon - PET</summary>
    ''' <param name="aTMinTS">Min Air Temperature - daily</param>
    ''' <param name="aTMaxTS">Max Air Temperature - daily</param>
    ''' <param name="aSource"></param>
    ''' <param name="aDegF">Temperature in Degrees F (True) or C (False)</param>
    ''' <param name="aLatDeg">Latitude, in degrees</param>
    ''' <param name="aCTS">Monthly variable coefficients</param>
    ''' <returns>Daily Pan Evaporation</returns>
    ''' <remarks></remarks>
    Public Function PanEvaporationTimeseriesComputedByHamon(ByVal datasrc As String,
                                                            ByVal aTMinTS As atcTimeseries,
                                                            ByVal aTMaxTS As atcTimeseries,
                                                            ByVal aSource As atcTimeseriesSource,
                                                            ByVal aDegF As Boolean,
                                                            ByVal aLatDeg As Double,
                                                            ByVal aCTS() As Double) As atcTimeseries
        Dim lAirTmp(aTMinTS.numValues) As Double
        Dim lPanEvp(aTMinTS.numValues) As Double
        Dim lPanEvapTimeSeries As New atcTimeseries(aSource)

        CopyBaseAttributes(aTMinTS, lPanEvapTimeSeries)
        lPanEvapTimeSeries.Attributes.SetValue("Constituent", "PET")
        lPanEvapTimeSeries.Attributes.SetValue("TSTYPE", "EVAP")
        lPanEvapTimeSeries.Attributes.SetValue("Scenario", "COMPUTED")
        lPanEvapTimeSeries.Attributes.SetValue("Description", "Daily Potential ET (in) computed using Hamon algorithm")
        lPanEvapTimeSeries.Attributes.AddHistory("Computed Daily Potential ET using Hamon - inputs: TMIN, TMAX, Degrees F, Latitude, Monthly Coefficients")
        lPanEvapTimeSeries.Attributes.Add("TMIN", aTMinTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("TMAX", aTMaxTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("Degrees F", aDegF)
        lPanEvapTimeSeries.Attributes.Add("LATDEG", aLatDeg)
        Dim lString As String = "("
        For lMonthIndex As Integer = 1 To 12
            lString &= aCTS(lMonthIndex) & ", "
        Next
        lString = lString.Substring(0, lString.Length - 2) & ")"
        lPanEvapTimeSeries.Attributes.Add("Monthly Coefficients", lString)
        lPanEvapTimeSeries.Dates = aTMinTS.Dates
        lPanEvapTimeSeries.numValues = aTMinTS.numValues

        'get fill value for input dsns
        Dim lMissingValue(2) As Double
        lMissingValue(1) = aTMinTS.Attributes.GetValue("TSFILL", -999)
        lMissingValue(2) = aTMaxTS.Attributes.GetValue("TSFILL", -999)

        Dim lDate(5) As Integer
        Dim lPoint As Boolean = aTMinTS.Attributes.GetValue("point", False)
        For lValueIndex As Integer = 1 To lPanEvapTimeSeries.numValues
            If Math.Abs(aTMinTS.Value(lValueIndex) - lMissingValue(1)) < 0.000001 OrElse
               Math.Abs(aTMaxTS.Value(lValueIndex) - lMissingValue(2)) < 0.000001 Then
                'missing source data
                lPanEvp(lValueIndex) = lMissingValue(1)
            Else 'compute pet
                lAirTmp(lValueIndex) = (aTMinTS.Value(lValueIndex) + aTMaxTS.Value(lValueIndex)) / 2
                If lPoint Then
                    J2Date(lPanEvapTimeSeries.Dates.Value(lValueIndex), lDate)
                Else
                    J2Date(lPanEvapTimeSeries.Dates.Value(lValueIndex - 1), lDate)
                End If
                lPanEvp(lValueIndex) = PanEvaporationValueComputedByHamon(lDate(1), lDate(2), aCTS, aLatDeg, lAirTmp(lValueIndex), aDegF, lMissingValue(1))
            End If
        Next lValueIndex
        Array.Copy(lPanEvp, 1, lPanEvapTimeSeries.Values, 1, lPanEvapTimeSeries.numValues)

        Return lPanEvapTimeSeries
    End Function
    ''' <summary>
    ''' Generates daily pan evaporation (inches) using a coefficient for the month, the possible hours of
    ''' sunshine (computed from latitude), and absolute humidity.
    ''' The computations are based on the Hamon (1961) formula.
    ''' </summary>
    ''' <param name="aMonth">Month</param>
    ''' <param name="aDay">Day</param>
    ''' <param name="aCTS">Array of monthly coefficients</param>
    ''' <param name="aLatDeg">Latitude, in degrees</param>
    ''' <param name="aTAVC">Average daily temperature (C)</param>
    ''' <param name="aDegF">Temperature in Fahrenheit (True) or Celsius (False)</param>
    ''' <param name="aMissingValue">Value to return if problem occurs</param>
    ''' <returns>Daily PET value</returns>
    ''' <remarks></remarks>
    Private Function PanEvaporationValueComputedByHamon(ByVal aMonth As Integer, ByVal aDay As Integer, ByVal aCTS() As Double, ByVal aLatDeg As Double, ByVal aTAVC As Double, ByVal aDegF As Boolean, ByVal aMissingValue As Double) As Double
        'check latitude
        If aLatDeg < MetComputeLatitudeMin OrElse aLatDeg > MetComputeLatitudeMax Then 'invalid latitude 
            Return aMissingValue
        Else 'latitude ok,convert to radians
            'TODO: make this consistant with our conventions
            Dim JulDay As Double = 30.5 * (aMonth - 1) + aDay

            Dim LatRdn As Double = aLatDeg * DegreesToRadians
            Dim Phi As Double = LatRdn
            Dim AD As Double = 0.40928 * System.Math.Cos(0.0172141 * (172.0# - JulDay))
            Dim SS As Double = System.Math.Sin(Phi) * System.Math.Sin(AD)
            Dim CS As Double = System.Math.Cos(Phi) * System.Math.Cos(AD)
            Dim X2 As Double = -SS / CS
            Dim Delt As Double = 7.6394 * (1.5708 - System.Math.Atan(X2 / System.Math.Sqrt(1.0# - X2 ^ 2)))
            Dim SunR As Double = 12.0# - Delt / 2.0#
            Dim SUNS As Double = 12.0# + Delt / 2.0#
            Dim DYL As Double = (SUNS - SunR) / 12

            'convert temperature to Centigrade if necessary
            If aDegF Then aTAVC = (aTAVC - 32.0#) * (5.0# / 9.0#)

            'Hamon equation
            Dim VPSAT As Double = 6.108 * System.Math.Exp(17.26939 * aTAVC / (aTAVC + 237.3))
            Dim VDSAT As Double = 216.7 * VPSAT / (aTAVC + 273.3)
            Dim lPanEvap As Double = aCTS(aMonth) * DYL * DYL * VDSAT

            'when the estimated pan evaporation is negative
            'the value is set to zero
            If lPanEvap < 0.0# Then
                lPanEvap = 0.0#
            End If
            Return lPanEvap
        End If
    End Function

    ''' <summary>
    ''' Compute daily solar radiation based on daily cloud cover
    ''' </summary>
    ''' <param name="aCldTSer">Cloud Cover timeseries</param>
    ''' <param name="aSource"></param>
    ''' <param name="aLatDeg">Latitude, in degrees</param>
    ''' <returns>Daily solar radiation timeseries</returns>
    ''' <remarks></remarks>

    Public Function SolarRadiationFromCloudCover(ByVal aCldTSer As atcTimeseries, ByVal aSource As atcTimeseriesSource, ByVal aLatDeg As Double) As atcTimeseries
        Dim lSolRadTs As New atcTimeseries(aSource)
        CopyBaseAttributes(aCldTSer, lSolRadTs)
        lSolRadTs.Attributes.SetValue("Constituent", "DSOL")
        lSolRadTs.Attributes.SetValue("TSTYPE", "DSOL")
        lSolRadTs.Attributes.SetValue("Scenario", "COMPUTED")
        lSolRadTs.Attributes.SetValue("Description", "Daily Solar Radiation (langleys) computed from Daily Cloud Cover")
        lSolRadTs.Attributes.AddHistory("Computed Daily Solar Radiation - inputs: DCLD, Latitude")
        lSolRadTs.Attributes.Add("DCLD", aCldTSer.ToString)
        lSolRadTs.Attributes.Add("Latitude", aLatDeg)
        lSolRadTs.Dates = aCldTSer.Dates
        lSolRadTs.numValues = aCldTSer.numValues
        Dim lCldCov(aCldTSer.numValues) As Double
        Array.Copy(aCldTSer.Values, 1, lCldCov, 1, aCldTSer.numValues)

        Dim lNaN As Double = GetNaN()
        Dim lDate(5) As Integer
        Dim lPoint As Boolean = aCldTSer.Attributes.GetValue("point", False)
        Dim lSolRad(aCldTSer.numValues) As Double

        For lValueIndex As Integer = 1 To lSolRadTs.numValues
            If Not Double.IsNaN(lCldCov(lValueIndex)) Then
                If lCldCov(lValueIndex) <= 0.0# Then lCldCov(lValueIndex) = 0.000001
                If lPoint Then
                    J2Date(aCldTSer.Dates.Value(lValueIndex), lDate)
                Else
                    J2Date(aCldTSer.Dates.Value(lValueIndex - 1), lDate)
                End If
                lSolRad(lValueIndex) = SolarRadiationValueFromCloudCover(aLatDeg, lCldCov(lValueIndex), lDate(1), lDate(2))
            Else
                lSolRad(lValueIndex) = lNaN
            End If
        Next lValueIndex
        Array.Copy(lSolRad, 1, lSolRadTs.Values, 1, lSolRadTs.numValues)

        Return lSolRadTs

    End Function
    ''' <summary>
    ''' Computes the total daily solar radiation based on
    ''' the HSPII (Hydrocomp, 1978) RADIATION procedure, which is based
    ''' on empirical curves of radiation as a function of latitude
    ''' (Hamon et al, 1954, Monthly Weather Review 82(6):141-146.
    ''' </summary>
    ''' <param name="aDegLat"></param>
    ''' <param name="aCloud"></param>
    ''' <param name="aMon"></param>
    ''' <param name="aDay"></param>
    ''' <remarks></remarks>
    Private Function SolarRadiationValueFromCloudCover(ByRef aDegLat As Double, ByRef aCloud As Double, ByRef aMon As Integer, ByRef aDay As Integer) As Double
        'integer part of latitude
        Dim lLatInt As Integer = Math.Floor(aDegLat)
        'fractional part of latitude
        Dim lLatFrac As Double = aDegLat - CSng(lLatInt)
        If lLatFrac <= 0.0001 Then lLatFrac = 0.0#

        Dim A0 As Double = XLax(lLatInt, 1) + lLatFrac * (XLax(lLatInt + 1, 1) - XLax(lLatInt, 1))
        Dim A1 As Double = XLax(lLatInt, 2) + lLatFrac * (XLax(lLatInt + 1, 2) - XLax(lLatInt, 2))
        Dim A2 As Double = XLax(lLatInt, 3) + lLatFrac * (XLax(lLatInt + 1, 3) - XLax(lLatInt, 3))
        Dim A3 As Double = XLax(lLatInt, 4) + lLatFrac * (XLax(lLatInt + 1, 4) - XLax(lLatInt, 4))
        Dim b1 As Double = XLax(lLatInt, 5) + lLatFrac * (XLax(lLatInt + 1, 5) - XLax(lLatInt, 5))
        Dim b2 As Double = XLax(lLatInt, 6) + lLatFrac * (XLax(lLatInt + 1, 6) - XLax(lLatInt, 6))
        Dim b As Double = aDegLat - 44.0#
        Dim a As Double = aDegLat - 25.0#
        Dim Exp1 As Double = 0.7575 - 0.0018 * a
        Dim Exp2 As Double = 0.725 + 0.00288 * b
        Dim Lat1 As Double = 2.139 + 0.0423 * a
        Dim Lat2 As Double = 30.0# - 0.667 * a
        Dim Lat3 As Double = 2.9 - 0.0629 * b
        Dim Lat4 As Double = 18.0# + 0.833 * b

        'Percent sunshine
        Dim SS As Double = 100.0# * (1.0# - (aCloud / 10.0#) ^ (5.0# / 3.0#))
        If SS < 0.0# Then 'can't have SS being negative
            SS = 0.0#
        End If

        Dim x As Double = X1(aMon) + aDay
        'convert to radians
        x *= DegreesToRadians

        Dim Y100 As Double = A0 + A1 * Math.Cos(x) + A2 * Math.Cos(2 * x) + A3 * Math.Cos(3 * x) + b1 * Math.Sin(x) + b2 * Math.Sin(2 * x)

        Dim ii As Double = Math.Ceiling((SS + 10.0#) / 10.0#)

        Dim YRD As Double
        If aDegLat > 43.0# Then
            YRD = Lat3 * SS ^ Exp2 + Lat4
        Else
            YRD = Lat1 * SS ^ Exp1 + Lat2
        End If

        If ii < 11 Then
            YRD += c(ii, aMon)
        End If

        Dim lDayRad As Double
        If YRD >= 100.0# Then
            lDayRad = Y100
        Else
            lDayRad = Y100 * YRD / 100.0#
        End If
        Return lDayRad
    End Function

#Region "Priestly-Taylor"
    ''' <summary>compute Priestly - PET</summary>
    ''' <param name="aTMinTS">Min Air Temperature - daily</param>
    ''' <param name="aTMaxTS">Max Air Temperature - daily</param>
    ''' <param name="aSource"></param>
    ''' <param name="aDegF">Temperature in Degrees F (True) or C (False)</param>
    ''' <param name="aLatDeg">Latitude, in degrees</param>
    ''' <returns>Daily Pan Evaporation</returns>
    ''' <remarks></remarks>
    Public Function PanEvaporationTimeseriesComputedByPriestly(ByVal aTMinTS As atcTimeseries,
                                                            ByVal aTMaxTS As atcTimeseries,
                                                            ByVal aSource As atcTimeseriesSource,
                                                            ByVal aDegF As Boolean,
                                                            ByVal aLatDeg As Double) As atcTimeseries
        Dim lAirTmp(aTMinTS.numValues) As Double
        Dim lPanEvp(aTMinTS.numValues) As Double
        Dim lPanEvapTimeSeries As New atcTimeseries(aSource)

        CopyBaseAttributes(aTMinTS, lPanEvapTimeSeries)
        lPanEvapTimeSeries.Attributes.SetValue("Constituent", "PEVT")
        lPanEvapTimeSeries.Attributes.SetValue("TSTYPE", "EVAP")
        lPanEvapTimeSeries.Attributes.SetValue("Scenario", "COMPUTED")
        lPanEvapTimeSeries.Attributes.SetValue("Description", "Daily Potential ET (in) computed using Priestley-Taylor algorithm")
        lPanEvapTimeSeries.Attributes.AddHistory("Computed Daily Potential ET using Priestley-Taylor, inputs: TMIN, TMAX, Degrees F, Latitude")
        lPanEvapTimeSeries.Attributes.Add("TMIN", aTMinTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("TMAX", aTMaxTS.ToString)
        lPanEvapTimeSeries.Attributes.Add("Degrees F", aDegF)
        lPanEvapTimeSeries.Attributes.Add("LATDEG", aLatDeg)
        Dim lString As String = "("

        lString = lString.Substring(0, lString.Length - 2) & ")"
        lPanEvapTimeSeries.Attributes.Add("Monthly Coefficients", lString)
        lPanEvapTimeSeries.Dates = aTMinTS.Dates
        lPanEvapTimeSeries.numValues = aTMinTS.numValues

        'get fill value for input dsns
        Dim lMissingValue(2) As Double
        lMissingValue(1) = aTMinTS.Attributes.GetValue("TSFILL", -999)
        lMissingValue(2) = aTMaxTS.Attributes.GetValue("TSFILL", -999)

        Dim lDate(5) As Integer
        Dim lPoint As Boolean = aTMinTS.Attributes.GetValue("point", False)
        For lValueIndex As Integer = 1 To lPanEvapTimeSeries.numValues
            If Math.Abs(aTMinTS.Value(lValueIndex) - lMissingValue(1)) < 0.000001 OrElse
               Math.Abs(aTMaxTS.Value(lValueIndex) - lMissingValue(2)) < 0.000001 Then
                'missing source data
                lPanEvp(lValueIndex) = lMissingValue(1)
            Else 'compute pet
                lAirTmp(lValueIndex) = (aTMinTS.Value(lValueIndex) + aTMaxTS.Value(lValueIndex)) / 2
                If lPoint Then
                    J2Date(lPanEvapTimeSeries.Dates.Value(lValueIndex), lDate)
                Else
                    J2Date(lPanEvapTimeSeries.Dates.Value(lValueIndex - 1), lDate)
                End If
                lPanEvp(lValueIndex) = PanEvaporationValueComputedByPriestly(lDate(1), lDate(2), aLatDeg, lAirTmp(lValueIndex), aDegF, lMissingValue(1))
            End If
        Next lValueIndex
        Array.Copy(lPanEvp, 1, lPanEvapTimeSeries.Values, 1, lPanEvapTimeSeries.numValues)

        Return lPanEvapTimeSeries
    End Function
    ''' <summary>
    ''' Generates daily pan evaporation (inches) using a coefficient for the month, the possible hours of
    ''' sunshine (computed from latitude), and absolute humidity.
    ''' The computations are based on the Hamon (1961) formula.
    ''' </summary>
    ''' <param name="aMonth">Month</param>
    ''' <param name="aDay">Day</param>
    ''' <param name="aLatDeg">Latitude, in degrees</param>
    ''' <param name="aTAVC">Average daily temperature (C)</param>
    ''' <param name="aDegF">Temperature in Fahrenheit (True) or Celsius (False)</param>
    ''' <param name="aMissingValue">Value to return if problem occurs</param>
    ''' <returns>Daily PET value</returns>
    ''' <remarks></remarks>
    Private Function PanEvaporationValueComputedByPriestly(ByVal aMonth As Integer, ByVal aDay As Integer, ByVal aLatDeg As Double, ByVal aTAVC As Double, ByVal aDegF As Boolean, ByVal aMissingValue As Double) As Double
        'check latitude
        If aLatDeg < MetComputeLatitudeMin OrElse aLatDeg > MetComputeLatitudeMax Then 'invalid latitude 
            Return aMissingValue
        Else 'latitude ok,convert to radians
            'TODO: make this consistant with our conventions
            Dim JulDay As Double = 30.5 * (aMonth - 1) + aDay

            Dim LatRdn As Double = aLatDeg * DegreesToRadians
            Dim Phi As Double = LatRdn
            Dim AD As Double = 0.40928 * System.Math.Cos(0.0172141 * (172.0# - JulDay))
            Dim SS As Double = System.Math.Sin(Phi) * System.Math.Sin(AD)
            Dim CS As Double = System.Math.Cos(Phi) * System.Math.Cos(AD)
            Dim X2 As Double = -SS / CS
            Dim Delt As Double = 7.6394 * (1.5708 - System.Math.Atan(X2 / System.Math.Sqrt(1.0# - X2 ^ 2)))
            Dim SunR As Double = 12.0# - Delt / 2.0#
            Dim SUNS As Double = 12.0# + Delt / 2.0#
            Dim DYL As Double = (SUNS - SunR) / 12

            'convert temperature to Centigrade if necessary
            If aDegF Then aTAVC = (aTAVC - 32.0#) * (5.0# / 9.0#)

            'Hamon equation
            Dim VPSAT As Double = 6.108 * System.Math.Exp(17.26939 * aTAVC / (aTAVC + 237.3))
            Dim VDSAT As Double = 216.7 * VPSAT / (aTAVC + 273.3)
            Dim lPanEvap As Double = DYL * DYL * VDSAT

            'when the estimated pan evaporation is negative
            'the value is set to zero
            If lPanEvap < 0.0# Then
                lPanEvap = 0.0#
            End If
            Return lPanEvap
        End If
    End Function
#End Region

End Module
