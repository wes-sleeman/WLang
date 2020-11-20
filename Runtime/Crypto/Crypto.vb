Imports System.Text.Encoding
Imports VBCrypto = System.Security.Cryptography

Public Module Crypto
	Private ReadOnly Sha256Instance As VBCrypto.SHA256 = VBCrypto.SHA256.Create()
	Private ReadOnly RandInstance As VBCrypto.RandomNumberGenerator = VBCrypto.RandomNumberGenerator.Create()

	''' <summary>
	''' Calculates the SHA256 hash of the provided inputs
	''' </summary>
	''' <param name="Data">Input values to be hased</param>
	''' <returns>A C() of hex-encoded SHA256 hashes</returns>
	Public Function SHA256(ParamArray Data())
		Dim retval As New List(Of String)

		For Each inp In Data
			retval.Add(
				Sha256Instance.ComputeHash(UTF8.GetBytes(inp.ToString())) _
				.Aggregate(String.Empty, Function(s, i) s & i.ToString("x2")))
		Next

		Return If(retval.Count = 1, retval(0), retval)
	End Function

	''' <summary>
	''' Returns cryptographically-secure random numbers between 0 and 255 inclusive.
	''' </summary>
	''' <param name="sizes">An optional list of sizes of output collections</param>
	''' <returns>A random number, collection of random numbers, or collection of collections of random numbers.</returns>
	Public Function Random(ParamArray sizes())
		If sizes.Length = 0 Then
			Dim byteArr(1) As Byte
			RandInstance.GetBytes(byteArr)
			Return byteArr(0)
		ElseIf sizes.Length = 1 Then
			Dim byteArr(UInteger.Parse(sizes(0).ToString)) As Byte
			RandInstance.GetBytes(byteArr)
			Return byteArr.ToList
		Else
			Dim retval As New List(Of List(Of Byte))
			For Each size In sizes
				Dim byteArr(UInteger.Parse(size.ToString)) As Byte
				RandInstance.GetBytes(byteArr)
				retval.Add(byteArr.ToList)
			Next
			Return retval
		End If
	End Function
End Module
