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
    | For | To | Step

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
type State = {
    IsFloatDetected: bool
    Segments: string list
    CurrentSegment: string
    lastResult: float
}

let initialState = { IsFloatDetected = false; Segments = []; CurrentSegment = ""; lastResult = 0.0 }
 

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

let addComplex (a: Complex) (b: Complex) =
    { Real = a.Real + b.Real; Imaginary = a.Imaginary + b.Imaginary }

let subComplex (a: Complex) (b: Complex) =
    { Real = a.Real - b.Real; Imaginary = a.Imaginary - b.Imaginary }

let mulComplex (a: Complex) (b: Complex) =
    { Real = a.Real * b.Real - a.Imaginary * b.Imaginary; 
      Imaginary = a.Real * b.Imaginary + a.Imaginary * b.Real }

let divComplex (a: Complex) (b: Complex) =
    let denom = b.Real ** 2.0 + b.Imaginary ** 2.0
    { Real = (a.Real * b.Real + a.Imaginary * b.Imaginary) / denom;
      Imaginary = (a.Imaginary * b.Real - a.Real * b.Imaginary) / denom }

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
        | 'f'::'o'::'r'::tail -> For :: scan tail  
        | 't'::'o'::tail -> To :: scan tail       
        | 's'::'t'::'e'::'p'::tail -> Step :: scan tail  
        | c :: tail when isblank c -> scan tail
        | c :: tail when isdigit c -> 
            let (iStr, iVal) = scInt(tail, intVal c)
            match iStr with
            | '.' :: rest -> 
                let (fStr, fVal) = scFloat(rest, float iVal, 10.0)              
                let updatedState = { initialState with IsFloatDetected = true } 
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
        | c :: tail when isdigit c || c = '+' || c = '-' -> 
            let rec readNumber chars acc =
                match chars with
                | h :: t when isdigit h || h = '.' -> readNumber t (acc + string h)
                | _ -> (chars, acc)
            let (remaining, realPart) = readNumber (c :: tail) ""
            match remaining with
            | '+' :: 'i' :: t -> NumComplex { Real = float realPart; Imaginary = 1.0 } :: scan t
            | '-' :: 'i' :: t -> NumComplex { Real = float realPart; Imaginary = -1.0 } :: scan t
            | 'i' :: t -> NumComplex { Real = 0.0; Imaginary = float realPart } :: scan t
            | _ -> NumFloat (float realPart) :: scan remaining
        | c :: tail -> Invalid c :: scan tail 
    scan (str2lst input)

let getInputString() : string = 
    Console.Write("Enter an expression: ")
    Console.ReadLine()

// Grammar in BNF:
//<E>          ::= <T> <Eopt>
//<Eopt>       ::= "+" <T> <Eopt> | "-" <T> <Eopt> | <empty>
//<T>          ::= <NR> <Topt>
//<Topt>       ::= "*" <NR> <Topt> | "/" <NR> <Topt> | <empty>
//<NR>         ::= "Num" <value> | <Complex> | "(" <E> ")"
//<P>        ::= "Num" <value>
//             | "(" <E> ")"
//             | "-" <P>
//             | <Function> <P>
//<Function> ::= "sin" | "cos" | "tan" | "exp" | "log"
//<Assignment> ::= <Variable> "=" <E>
//<Rational> ::= "Num" <Numerator> "/" <Denominator>
//<Variable> ::= <Identifier>
//<Complex>    ::= <RealPart> "+" <ImagPart> "i" | <RealPart> "-" <ImagPart> "i"
//<RealPart>   ::= <value>
//<ImagPart>   ::= <value>
//<ForLoop>    ::= "for" <Variable> "=" <Start> "to" <End> "step" <Step> "do" <E>
//<Start>      ::= <value>
//<End>        ::= <value>
//<Step>       ::= <value>

let parser tList = 
    let rec E tList = (T >> Eopt) tList         // >> is forward function composition operator: let inline (>>) f g x = g(f x)
    and Eopt tList = 
        match tList with
        | Add :: tail -> (T >> Eopt) tail
        | Sub :: tail -> (T >> Eopt) tail
        | _ -> tList
    and T tList = (NR >> Topt) tList
    and Topt tList =
        match tList with
        | Mul :: tail -> (NR >> Topt) tail
        | Div :: tail -> (NR >> Topt) tail
        | _ -> tList
    and NR tList =
        match tList with 
        | NumInt value :: tail -> tail
        | Lpar :: tail -> match E tail with 
                          | Rpar :: tail -> tail
                          | _ -> raise parseError
        | _ -> raise parseError
    E tList

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
        //| Add :: NumComplex c :: tail -> Fopt (tail, addComplex value c)
        //| Sub :: NumComplex c :: tail -> Fopt (tail, subComplex value c)
        //| Mul :: NumComplex c :: tail -> Fopt (tail, mulComplex value c)
        //| Div :: NumComplex c :: tail -> Fopt (tail, divComplex value c)    
        | _ -> (tList, value)
    and P tList = 
        match tList with 
        | NumInt value :: tail -> (tail, float value)
        | NumFloat value :: tail -> (tail, value)
        | NumRational { Numerator = num; Denominator = denom } :: tail -> 
            (tail, float num / float denom)
        //| NumComplex { Real = real; Imaginary = imag } :: tail ->
        //    (tail, (real, imag))  
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
        | _ ->  raise parseError
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


let rec parseForLoop tokens =
    let tokenString = String.Join(" ", List.map (fun t -> t.ToString()) tokens)
    System.Diagnostics.Debug.WriteLine("Parsing tokens: " + tokenString)

    match tokens with
    | For :: Variable varName :: To :: NumInt start :: Step :: NumInt step :: tail ->
        let loopCode = 
            [Variable varName; Assign; NumInt start; To; NumInt (start + step); Step; NumInt step; Lpar] 
        loopCode @ tail  
    | _ -> tokens  



let splitString (delimiter: char) (input: string) =
    let rec processInput state input =
        match input with
        | [] -> 
            
            if state.CurrentSegment <> "" then
                { state with Segments = state.CurrentSegment :: state.Segments }
            else
                state  
        | c :: rest -> 
            if c = delimiter then
                
                let updatedState = 
                    if state.CurrentSegment <> "" then
                        { state with Segments = state.CurrentSegment :: state.Segments; CurrentSegment = "" }
                    else
                        state
                processInput updatedState rest
            else
                
                processInput { state with CurrentSegment = state.CurrentSegment + string c } rest

    let finalState = processInput initialState (List.ofSeq input)

    List.rev finalState.Segments


let evaluateExpression (input: string) : string =
    let statements = splitString ';' input

    let processStatement (state: State) (statement: string) : State =
        let trimmedStatement = statement.Trim()
        if trimmedStatement <> "" then
            let tokenList = lexer trimmedStatement
            let (parsedList, result) = parseAssignment tokenList

            validateTokens tokenList parsedList
            System.Diagnostics.Debug.WriteLine(result)

            { state with lastResult = result }
        else
            state

    let finalState =
        statements
        |> List.fold processStatement initialState

    let formattedResult =
        if initialState.IsFloatDetected then
            sprintf "%.2f" finalState.lastResult
        else
            finalState.lastResult.ToString()

    System.Diagnostics.Debug.WriteLine(formattedResult)

    formattedResult


let evaluatePolynomial (input: string) : string =
        let tokenList = lexer input

        //validateTokensForPolynomials tokenList

        "Evaluated polynomial"

let evaluatePolynomialForLoop (input: string) (expression: string) : float list * float list =
    if input.Contains("x") then
        let tokenListForLoop = lexer input
        let loopCode = parseForLoop tokenListForLoop

        if (List.exists (fun token -> token = For) loopCode) then
            let extractLoopValues tokenListForLoop =
                System.Diagnostics.Debug.WriteLine($"Token List: {tokenListForLoop}")
                match tokenListForLoop with
                | [For; Variable _; Assign; NumInt startX; To; NumInt endX; Step; NumInt step] ->
                    (float startX, float endX, float step) 
                | [For; Variable _; Assign; NumInt startX; To; NumInt endX; Step; NumFloat step] ->
                    (float startX, float endX, step) 
                | [For; Variable _; Assign; NumFloat startX; To; NumFloat endX; Step; NumFloat step] ->
                    (startX, endX, step) 
                | [For; Variable _; Assign; Sub; NumInt startX; To; NumInt endX; Step; NumInt step] ->
                    (-float startX, float endX, float step) 
                | [For; Variable _; Assign; Sub; NumInt startX; To; NumInt endX; Step; NumFloat step] ->
                    (-float startX, float endX, float step)
                | [For; Variable _; Assign; Sub; NumFloat startX; To; NumFloat endX; Step; NumFloat step] ->
                    (-startX, endX, step) 
                | [For; Variable _; Assign; NumInt startX; To; Sub; NumInt endX; Step; NumInt step] ->
                    (float startX, -float endX, float step) 
                | [For; Variable _; Assign; NumFloat startX; To; Sub; NumFloat endX; Step; NumFloat step] ->
                    (startX, -endX, step) 
                 | [For; Variable _; Assign; NumInt startX; To; NumInt endX; Step; Sub; NumInt step] ->
                    (float startX, float endX, -float step) 
                | [For; Variable _; Assign; NumFloat startX; To; NumFloat endX; Step; Sub; NumFloat step] ->
                    (startX, endX, - step) 
                | _ ->
                    failwithf "Unexpected loop code format: %A" tokenListForLoop

            let (startX, endX, step) = extractLoopValues loopCode
            System.Diagnostics.Debug.WriteLine($"startX: {startX}, endX: {endX}, step: {step}")

            let rec interpolate currentX acc =
                if currentX > endX then
                    acc  
                else
                    let currentExpression = expression.Replace("x", currentX.ToString("G")) 
                    
                    let y = evaluateExpression currentExpression |> float

                    interpolate (currentX + step) ((currentX, y) :: acc)

            let points = interpolate startX [] |> List.rev  

            let xValues = List.map fst points
            let yValues = List.map snd points

            System.Diagnostics.Debug.WriteLine($"xValues: {xValues}")
            System.Diagnostics.Debug.WriteLine($"yValues: {yValues}")

            (xValues, yValues)
        else
            failwith "Invalid for loop statement"
    else
        failwith "Input is not a polynomial"




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
