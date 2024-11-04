module ArithmeticInterpreter

open System



type Rational = { Numerator: int; Denominator: int }
type Complex = { Real: float; Imaginary: float }

type terminal = 
    | Add | Sub | Mul | Div | Mod | Pow | Cos | Sin | Tan | Exp | Log | Lpar | Rpar 
    | NumInt of int 
    | NumFloat of float 
    | NumRational of Rational
    | NumComplex of Complex
    | Variable of string
    | Assign
    | Invalid of char

let mutable symbolTable = []

let setVariable name value =
    symbolTable <- (name, value) :: List.filter (fun (n, _) -> n <> name) symbolTable

let getVariable name =
    match List.tryFind (fun (n, _) -> n = name) symbolTable with
    | Some (_, value) -> value
    | None -> raise (System.Exception(sprintf "Variable '%s' is not defined" name))

let str2lst s = [for c in s -> c]
let isblank c = System.Char.IsWhiteSpace c
let isdigit c = System.Char.IsDigit c
let isalpha c = System.Char.IsLetter c
let lexError = System.Exception("Lexer error")
let intVal (c:char) = (int)((int)c - (int)'0')
let parseError = System.Exception("Parser error")
let mutable isFloatDetected = false

let rec power baseVal exponent =
    match exponent with
    | exp when exp < 0.0 -> 1.0 / (power baseVal (-exp))  
    | 0.0 -> 1.0  
    | 1.0 -> baseVal 
    | exp when exp % 2.0 = 0.0 -> 
        let halfPower = power baseVal (exp / 2.0)
        halfPower * halfPower  
    | _ -> baseVal * power baseVal (exponent - 1.0) 

let rec scInt(iStr, iVal) = 
    match iStr with
    | c :: tail when isdigit c -> scInt(tail, 10 * iVal + intVal c)
    | _ -> (iStr, iVal)

let rec scFloat(fStr, fVal, divisor) =
    match fStr with
    | c :: tail when isdigit c -> scFloat(tail, fVal + float (intVal c) / divisor, divisor * 10.0)
    | _ -> scExp(fStr, fVal)

and scExp(fStr, fVal) =
    match fStr with
    | 'E' :: '+' :: tail -> scInt(tail, 0) |> fun (rest, exp) -> (rest, fVal * power 10.0 (float exp))
    | 'E' :: '-' :: tail -> scInt(tail, 0) |> fun (rest, exp) -> (rest, fVal * power 10.0 (float -exp))
    | 'E' :: tail -> scInt(tail, 0) |> fun (rest, exp) -> (rest, fVal * power 10.0 (float exp))
    | _ -> (fStr, fVal)

let lexer input = 
    let rec scan input =
        match input with
        | [] -> []
        | '+'::tail -> Add :: scan tail
        | '-'::tail -> Sub :: scan tail
        | '*'::tail -> Mul :: scan tail
        | '/'::tail -> Div :: scan tail
        | '%'::tail -> Mod :: scan tail
        | '^'::tail -> Pow :: scan tail
        | '('::tail -> Lpar :: scan tail
        | ')'::tail -> Rpar :: scan tail
        | '='::tail -> Assign :: scan tail
        | 's'::'i'::'n'::tail -> Sin :: scan tail
        | 'c'::'o'::'s'::tail -> Cos :: scan tail
        | 't'::'a'::'n'::tail -> Tan :: scan tail
        | 'e'::'x'::'p'::tail -> Exp :: scan tail
        | 'l'::'o'::'g'::tail -> Log :: scan tail
        | c :: tail when isblank c -> scan tail
        | c :: tail when isdigit c -> 
            let (iStr, iVal) = scInt(tail, intVal c)
            match iStr with
            | '.' :: rest -> 
                let (fStr, fVal) = scFloat(rest, float iVal, 10.0)              
                isFloatDetected <- true 
                NumFloat fVal :: scan fStr
            | 'E' :: _ -> 
                let (expStr, expVal) = scExp(tail, float iVal)
                NumFloat expVal :: scan expStr
            | '/'::tail2 when isdigit (List.head tail2) -> 
                let (rest, denom) = scInt(tail2, 0)
                NumRational { Numerator = iVal; Denominator = denom } :: scan rest
            | _ -> NumInt iVal :: scan iStr
        | c :: tail when isalpha c -> 
            let rec readVar chars name =
                match chars with
                | h :: t when isalpha h || isdigit h -> readVar t (name + string h)
                | _ -> (chars, name)
            let (remaining, varName) = readVar tail (string c)
            Variable varName :: scan remaining
        | c :: tail when c = 'i' -> 
            let (rest, imag) = scFloat(tail, 0.0, 10.0)
            NumComplex { Real = 0.0; Imaginary = imag } :: scan rest
        | c :: tail -> Invalid c :: scan tail 
    scan (str2lst input)

let rec parseNeval tList = 
    let rec E tList = (T >> Eopt) tList
    and Eopt (tList, value) = 
        match tList with
        | Add :: tail -> 
            let (tLst, tval) = T tail
            Eopt (tLst, value + tval)
        | Sub :: tail -> 
            let (tLst, tval) = T tail
            Eopt (tLst, value - tval)
        | _ -> (tList, value)
    and T tList = (F >> Topt) tList
    and Topt (tList, value) =
        match tList with
        | Mul :: tail -> 
            let (tLst, tval) = F tail
            Topt (tLst, value * tval)
        | Div :: tail -> 
            let (tLst, tval) = F tail
            Topt (tLst, value / tval)
        | Mod :: tail -> 
            let (tLst, tval) = F tail
            Topt (tLst, value % tval)
        | _ -> (tList, value)
    and F tList = (P >> Fopt) tList
    and Fopt (tList, value) =
        match tList with
        | Pow :: tail ->  
            let (tLst, tval) = P tail
            Fopt (tLst, power value tval)  
        | _ -> (tList, value)
    and P tList = 
        match tList with 
        | NumInt value :: tail -> (tail, float value)
        | NumFloat value :: tail -> (tail, value)
        | NumRational { Numerator = num; Denominator = denom } :: tail -> 
            (tail, float num / float denom)
        //| NumComplex { Real = real; Imaginary = imag } :: tail -> 
            //(tail, Complex(real, imag))
        | Variable varName :: tail -> 
            (tail, getVariable varName)
        | Lpar :: tail -> 
            let (tLst, tval) = E tail
            match tLst with 
            | Rpar :: tail -> (tail, tval)
            | _ -> raise parseError  
        | Sub :: tail -> 
            let (tLst, tval) = P tail 
            (tLst, -tval)
        | Cos :: tail -> 
            let (tLst, tval) = P tail
            (tLst, Math.Cos(tval))
        | Sin :: tail -> 
            let (tLst, tval) = P tail
            (tLst, Math.Sin(tval))
        | Tan :: tail -> 
            let (tLst, tval) = P tail
            (tLst, Math.Tan(tval))
        | Exp :: tail -> 
            let (tLst, tval) = P tail
            (tLst, Math.Exp(tval))
        | Log :: tail -> 
            let (tLst, tval) = P tail
            (tLst, Math.Log(tval))
        | _ -> raise parseError
    E tList

let parseAssignment tList =
    match tList with
    | Variable varName :: Assign :: tail -> 
        let (remaining, value) = parseNeval tail
        setVariable varName value
        remaining, value
    | _ -> parseNeval tList

let validateTokens tokenList parsedList =
    match parsedList with
    | [] -> ()  
    | Invalid c :: _ -> raise (System.Exception(sprintf "Invalid character: '%c'" c))
    | _ -> raise parseError  

let splitString (delimiter: char) (input: string) =
    let mutable segments = []
    let mutable currentSegment = ""

    for c in input do
        if c = delimiter then
            if currentSegment <> "" then
                segments <- currentSegment :: segments
                currentSegment <- ""  
        else
            currentSegment <- currentSegment + string c 

    if currentSegment <> "" then
        segments <- currentSegment :: segments

    List.rev segments

let evaluateExpression (input: string) : string =
    let statements = splitString ';' input
    let mutable lastResult: float = 0.0  

    for statement in statements do
        let trimmedStatement = statement.Trim()
        if trimmedStatement <> "" then
            let tokenList = lexer trimmedStatement
            let (parsedList, result) = parseAssignment tokenList

            validateTokens tokenList parsedList
            lastResult <- result  
            System.Diagnostics.Debug.WriteLine(result)

    let formattedResult =
        if isFloatDetected then
            sprintf "%.2f" lastResult 
        else
            lastResult.ToString() 

    System.Diagnostics.Debug.WriteLine(formattedResult)  
    isFloatDetected <- false  
    formattedResult  

let helpInfo () =
    let info = """
    Valid Tokens:
    - Operators: 
        + (Add), - (Subtract), * (Multiply), / (Divide), % (Modulus), ^ (Power), E (Exponential)
    - Trigonometric and Math Functions:
        cos, sin, tan, exp, log
    - Parentheses: 
        ( ) for grouping expressions
    - Assignment: 
        = to assign values to variables
    - Numbers: 
        - Integers (e.g., 42)
        - Floating-point (e.g., 3.14)
        - Rational (e.g., 3/4)
        - Complex (e.g., 1 + 2i) - Under development
    - Variables: 
        Alphanumeric names starting with a letter (e.g., x, myVar)
    
    Syntax:
    - Expressions can include numbers, variables, operators, and functions.
    - Example of an expression: (3 + 4) * x - 2.5
    - Variable assignment: x = 5
    - Multiple statements can be separated by semicolons: x = 5; y = 3 + x; z = y * 2
    - Exponential notation: 2E5 represents 2 * 10^5. Using Floating Points: Under development
    - Rational numbers: Use the format a/b for fractions (e.g., 3/4 for three-quarters). Using Floating points - Under Development
    - Complex numbers: Enter as a + bi (e.g., 1 + 2i for the complex number 1 + 2i) - Under Development
    
    Trigonometric and Math Functions:
    - Trigonometric functions: sin(x), cos(x), tan(x)
        - Example: sin(0) returns 0.0
        - Input values are in radians; e.g., cos(3.14159 / 2) is approximately 0.
    - Exponential and Logarithmic functions:
        - exp(x): Calculates e^x, where e ≈ 2.718
        - log(x): Calculates the natural logarithm of x
        - Example: exp(1) returns approximately 2.718
    
    Notes:
    - Use parentheses for function arguments: e.g., cos(0), exp(1).
    - Variables, rational numbers, and complex numbers can be used within expressions.
    """
    info

