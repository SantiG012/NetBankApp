﻿using NetBank.Domain.Dto;
using System.Text;

namespace NetBank.Utilities;

public static class CreditCardValidator
{
    private const int MAX_VALUE_DIGIT = 9;
    private const int MIN_LENGTH = 13;
    private const int MAX_LENGTH = 19;

    public static bool IsValid(string creditCardNumber)
    {
        int sum = 0;
        int digit = 0;
        int addend = 0;
        bool timesTwo = false;

        var digitsOnly = GetDigits(creditCardNumber);

        if (digitsOnly.Length > MAX_LENGTH || digitsOnly.Length < MIN_LENGTH) return false;

        for (var i = digitsOnly.Length - 1; i >= 0; i--)
        {
            digit = int.Parse(digitsOnly.ToString(i, 1));
            if (timesTwo)
            {
                addend = digit * 2;
                if (addend > MAX_VALUE_DIGIT)
                    addend -= MAX_VALUE_DIGIT;
            }
            else
                addend = digit;

            sum += addend;

            timesTwo = !timesTwo;
        }
        return (sum % 10) == 0;
    }

    private static StringBuilder GetDigits(string creditCardNumber)
    {
        var digitsOnly = new StringBuilder();
        foreach (var character in creditCardNumber)
        {
            if (char.IsDigit(character))
                digitsOnly.Append(character);
        }
        return digitsOnly;
    }

    public static bool IsValidCreditCardLength(string creditCardNumber, List<IssuingNetworkData> issuingNetworkDataList, string issuingNetworkName)
    {
        IssuingNetworkData issuingNetworkData = issuingNetworkDataList.Single(issuingNetworkData => issuingNetworkData.Name.Trim() == issuingNetworkName.Trim());//Finds the data of the issuing network
        int creditCardNumberLength = creditCardNumber.Trim().Length;

        return issuingNetworkData.AllowedLengths.Contains(creditCardNumberLength);
    }

    public static string FindIssuingNetwork(string creditCardNumber, List<IssuingNetworkData> issuingNetworkDataList)
    {
        string issuingNetworkName = "";
        bool startsWithNumbers = false;
        bool startsWithNumbersInRange = false;

        foreach (IssuingNetworkData issuingNetworkData in issuingNetworkDataList)
        {
            if (issuingNetworkData.StartsWithNumbers?.Any() ?? false)
            {
                startsWithNumbers = StartsWithNumberFromList(creditCardNumber, issuingNetworkData.StartsWithNumbers);
            }

            if (issuingNetworkData.InRange != null)
            {
                startsWithNumbersInRange = StartsWithNumberInRange(creditCardNumber, issuingNetworkData.InRange);
            }

            if (startsWithNumbers || startsWithNumbersInRange)
            {
                issuingNetworkName = issuingNetworkData.Name;
                break;
            }
        }

        return issuingNetworkName;
    }

    private static bool StartsWithNumberFromList(string creditCardNumber, List<int> numberList)
    {
        //Checks if the creadit card number starts with any number inside a particular list of numbers
        int numberLength;
        int initalCreditCardDigits;

        foreach (int number in numberList)
        {
            numberLength = number.ToString().Length;

            if (creditCardNumber.Length < numberLength) { continue; } //If the length of the credit card number is shorter than expected, those numbers will never be equal

            initalCreditCardDigits = int.Parse(creditCardNumber.Substring(0, numberLength));//Takes the initial digits of the credit card 
            
            if (initalCreditCardDigits == number)//Compares the inital digits of the credit card with each element of the list
            {
                return true;
            }
        }

        return false;
    }

    private static bool StartsWithNumberInRange(string creditCardNumber, RangeNumber rangeNumber)
    {
        int numberLength = rangeNumber.MinValue.ToString().Length;

        if (creditCardNumber.Length < numberLength) { return false; }//If the length of the credit card number is shorter than the minimum allowable length, then the credit card number is out of range

        int initalCreditCardDigits = int.Parse(creditCardNumber[..numberLength]);//Takes the initial digits of the credit card 
        
        //Checks if the initial digits of the credit card number are between a range
        return (initalCreditCardDigits >= rangeNumber.MinValue) && (initalCreditCardDigits <= rangeNumber.MaxValue);
    }
}