module ArithmeticInterpreter

open System
open System.Text.RegularExpressions
open System.IO
open System.Diagnostics


open MathNet.Numerics.Interpolation;




type Rational = { Numerator: int; Denominator: int }
type Complex = { Real: float; Imaginary: float }

type terminal = 
    | Add | Sub | Mul | Div | Mod | Pow | Cos | Sin | Tan | Exp | Log | Lpar | Rpar | Sqrt
    | NumInt of int 
    | NumFloat of float 
    | NumRational of Rational
    | NumComplex of Complex
    | Variable of string
    | Assign
    | Invalid of char
    | For | To | Step
    | Derivative of string
    | IntegralStart
    | Comma


// Define the expression types with new constructors
type Expr =
    | Const of float
    | Var of string
    | AddCalculus of Expr * Expr
    | SubCalculus of Expr * Expr
    | MulCalculus of Expr * Expr
    | DivCalculus of Expr * Expr
    | PowCalculus of Expr * Expr
    | SinCalculus of Expr
    | CosCalculus of Expr
    | TanCalculus of Expr
    | ExpCalculus of Expr
    | LogCalculus of Expr
    | Integral of Expr * Expr * Expr

// Differentiation function
let rec differentiate expr var =
    match expr with
    | Const _ -> Const 0.0
    | Var v -> if v = var then Const 1.0 else Const 0.0
    | AddCalculus (u, v) -> AddCalculus (differentiate u var, differentiate v var)
    | SubCalculus (u, v) -> SubCalculus (differentiate u var, differentiate v var)
    | MulCalculus (u, v) -> 
        AddCalculus (MulCalculus (differentiate u var, v), MulCalculus (u, differentiate v var))
    | DivCalculus (u, v) ->
        DivCalculus (
            SubCalculus (
                MulCalculus (differentiate u var, v),
                MulCalculus (u, differentiate v var)
            ),
            PowCalculus (v, Const 2.0)
        )
    | PowCalculus (u, Const n) -> 
        // Apply the power rule correctly: n * u^(n-1) * du/dx
        MulCalculus (
            Const n,
            MulCalculus (
                PowCalculus (u, Const (n - 1.0)),
                differentiate u var
            )
        )
    | SinCalculus u -> 
        MulCalculus (CosCalculus u, differentiate u var)
    | CosCalculus u -> 
        MulCalculus (Const -1.0, MulCalculus (SinCalculus u, differentiate u var))
    | TanCalculus u -> 
        DivCalculus (differentiate u var, PowCalculus (CosCalculus u, Const 2.0))
    | ExpCalculus u -> 
        MulCalculus (ExpCalculus u, differentiate u var)
    | LogCalculus u -> 
        DivCalculus (differentiate u var, u)
    | _ -> failwith "Non-constant exponent differentiation not implemented"

// Enhanced simplification function
let rec simplify expr =
    match expr with
    | AddCalculus (Const 0.0, e) | AddCalculus (e, Const 0.0) -> simplify e
    | AddCalculus (Const a, Const b) -> Const (a + b)
    | AddCalculus (e1, e2) ->
        let s1 = simplify e1
        let s2 = simplify e2
        match s1, s2 with
        | Const 0.0, e | e, Const 0.0 -> simplify e
        | Const a, Const b -> Const (a + b)
        | _ -> AddCalculus (s1, s2)
    | SubCalculus (e, Const 0.0) -> simplify e
    | SubCalculus (Const a, Const b) -> Const (a - b)
    | SubCalculus (e1, e2) -> 
        let s1 = simplify e1
        let s2 = simplify e2
        SubCalculus (s1, s2)
    | MulCalculus (Const 0.0, _) | MulCalculus (_, Const 0.0) -> Const 0.0
    | MulCalculus (Const 1.0, e) | MulCalculus (e, Const 1.0) -> simplify e
    | MulCalculus (Const a, Const b) -> Const (a * b)
    | MulCalculus (e1, e2) -> 
        let s1 = simplify e1
        let s2 = simplify e2
        MulCalculus (s1, s2)
    | DivCalculus (Const 0.0, _) -> Const 0.0
    | DivCalculus (e, Const 1.0) -> simplify e
    | DivCalculus (Const a, Const b) -> Const (a / b)
    | DivCalculus (e1, e2) -> 
        let s1 = simplify e1
        let s2 = simplify e2
        DivCalculus (s1, s2)
    | PowCalculus (e, Const 1.0) -> simplify e
    | PowCalculus (_, Const 0.0) -> Const 1.0
    | PowCalculus (Const a, Const b) -> Const (a ** b)
    | PowCalculus (e1, e2) -> 
        let s1 = simplify e1
        let s2 = simplify e2
        PowCalculus (s1, s2)
    | SinCalculus e -> SinCalculus (simplify e)
    | CosCalculus e -> CosCalculus (simplify e)
    | TanCalculus e -> TanCalculus (simplify e)
    | ExpCalculus e -> ExpCalculus (simplify e)
    | LogCalculus e -> LogCalculus (simplify e)
    | _ -> expr

// Convert the expression to a string
let rec exprToString expr =
    match expr with
    | Const c -> 
        if c % 1.0 = 0.0 then string (int c) else string c
    | Var v -> v
    | AddCalculus (e1, e2) -> sprintf "%s + %s" (exprToString e1) (exprToString e2)
    | SubCalculus (e1, e2) -> sprintf "%s - %s" (exprToString e1) (exprToString e2)
    | MulCalculus (Const c, Var v) | MulCalculus (Var v, Const c) -> 
        if c = 1.0 then v else sprintf "%s%s" (exprToString (Const c)) v
    | MulCalculus (e1, e2) -> sprintf "%s * %s" (exprToString e1) (exprToString e2)
    | DivCalculus (e1, e2) -> sprintf "%s / %s" (exprToString e1) (exprToString e2)
    | PowCalculus (e, Const n) -> sprintf "%s^%s" (exprToString e) (exprToString (Const n))
    | SinCalculus e -> sprintf "sin(%s)" (exprToString e)
    | CosCalculus e -> sprintf "cos(%s)" (exprToString e)
    | TanCalculus e -> sprintf "tan(%s)" (exprToString e)
    | ExpCalculus e -> sprintf "exp(%s)" (exprToString e)
    | LogCalculus e -> sprintf "log(%s)" (exprToString e)
    | _ -> failwith "Non-constant exponent printing not implemented"

// Example usage
let expr = PowCalculus (Var "x", Const 3.0)
let derivative = differentiate expr "x"
let simplifiedDerivative = simplify derivative
let result = exprToString simplifiedDerivative

System.Diagnostics.Debug.WriteLine("Derivative: " + result)

let trapezoidalRule (f: float -> float) (a: float) (b: float) (n: int) =
    let h = (b - a) / float n
    let mutable result = (f a + f b) / 2.0
    for i = 1 to n - 1 do
        result <- result + f (a + h * float i)
    result * h

// Evaluate expressions
let rec evaluate expr (var: string) (value: float) =
    match expr with
    | Const c -> c
    | Var v -> if v = var then value else failwith "Variable mismatch"
    | AddCalculus (u, v) -> evaluate u var value + evaluate v var value
    | SubCalculus (u, v) -> evaluate u var value - evaluate v var value
    | MulCalculus (u, v) -> evaluate u var value * evaluate v var value
    | DivCalculus (u, v) -> evaluate u var value / evaluate v var value
    | PowCalculus (u, v) -> evaluate u var value ** evaluate v var value
    | SinCalculus u -> Math.Sin (evaluate u var value)
    | CosCalculus u -> Math.Cos (evaluate u var value)
    | ExpCalculus u -> Math.Exp (evaluate u var value)
    | LogCalculus u -> Math.Log (evaluate u var value)
    | Integral (a, b, f) ->
        let lower = evaluate a var value
        let upper = evaluate b var value
        trapezoidalRule (fun x -> evaluate f var x) lower upper 1000



//let rec newtonRaphson expr var x0 tol maxIter =
//    let rec iterate x iter =
//        if iter >= maxIter then x
//        else
//            let fx = exprToString (simplify expr)
//            let derivative = differentiate expr "x"
//            let simplifiedDerivative = simplify derivative
//            let dfx = exprToString simplifiedDerivative
//            let nextX = x - (fx |> float) / (dfx |> float)
//            if abs (nextX - x) < tol then nextX
//            else iterate nextX (iter + 1)
//    iterate x0 0

//let root = newtonRaphson expr "x" 2.0 0.0001 100


//let rec evaluate expr (var: string) (value: float) =
//    match expr with
//    | Const c -> c
//    | Integral (f, v, a, b) -> integrate f v a b
//    | _ -> failwith "Unsupported operation"

//// Numerical integration using the Trapezoidal Rule
//and integrate (f: Expr) (v: string) (a: float) (b: float) =
//    let n = 1000 // Number of subintervals
//    let h = (b - a) / float n
//    let mutable total = 0.0

//    for i = 0 to n do
//        let x = a + h * float i
//        let weight = if i = 0 || i = n then 0.5 else 1.0
//        total <- total + weight * evaluate f v x

//    h * total

// Example: Integral of x^2 from 0 to 1
let exampleExpr = PowCalculus (Var "x", Const 2.0)
//let integralExpr = Integral (exampleExpr, "x", 0.0, 1.0)
//let integralResult = evaluate integralExpr "x" 0.0 // The variable value is unused for integration
System.Diagnostics.Debug.WriteLine("Integral Result: " + exampleExpr.ToString())

//let trapezoidalRule f a b n =
//    // Step size
//    let h = (b - a) / float n
//    // Compute the sum of the first and last terms
//    let mutable result = (f a + f b) / 2.0
//    // Sum the values at intermediate points
//    for i in 1 .. (n - 1) do
//        let x = a + float i * h
//        result <- result + f x
//    // Multiply by the step size
//    result * h

    // Function to integrate
//let f x = x^3.0 + x^2.0 + 2.0 * x + 3.0

// Limits and intervals
let a = 0.0
let b = 2.0
let n = 4

// Compute definite integral
//let result1 = trapezoidalRule f a b n

// Print the result
//System.Diagnostics.Debug.WriteLine("Definite Integral of f(x) = x^2" + result1.ToString())



let boolArray = [| false; false |]

let mutable symbolTable = []

let setVariable name value =
    symbolTable <- (name, value) :: List.filter (fun (n, _) -> n <> name) symbolTable

//let getVariable name =
//    match (boolArray.[0], List.tryFind (fun (n, _) -> n = name) symbolTable) with
//    | (true, None) -> 0.0
//    | (false, Some (_, value)) -> value
//    | (false, None) -> raise (System.Exception(sprintf "Variable '%s' is not defined" name))


let getVariable name =
    match List.tryFind (fun (n, _) -> n = name) symbolTable with
    | Some (_, value) -> value
        
    | None -> raise (System.Exception(sprintf "Variable '%s' is not defined" name))

let getAllVariables () =
    symbolTable
    |> List.map (fun (name, value) -> sprintf "%s = %s" name (value.ToString()))
    |> String.concat "\n"

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
        | 's'::'q'::'r'::'t'::tail -> Sqrt :: scan tail 
        | 'd' :: '/' :: 'd' :: var :: '(' :: tail when Char.IsLetter var ->
                Derivative (string var) :: Lpar :: scan tail
        | ('i'|'I')::'n'::'t'::'e'::'g'::'r'::'a'::'l'::'('::tail -> IntegralStart :: scan tail
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
        | ',' :: tail -> Comma :: scan tail
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
            (tLst, Math.Cos(tval)) //Math.Cos(tval* Math.PI / 180.0) for degree. But wont give a sin wave in the plot
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
        | Sqrt :: tail -> 
            let (tLst, tval) = P tail
            (tLst, Math.Sqrt(tval))
        | _ ->  raise parseError
    E tList

let rec parseExpr tList =
    let rec E tList = (T >> Eopt) tList
    and Eopt (tList, expr) =
        match tList with
        | Add :: tail ->
            let (tLst, rhs) = T tail
            Eopt (tLst, AddCalculus (expr, rhs))
        | Sub :: tail ->
            let (tLst, rhs) = T tail
            Eopt (tLst, SubCalculus (expr, rhs))
        | _ -> (tList, expr)

    and T tList =
        let (tLst, lhs) = F tList
        Topt (tLst, lhs)

    and Topt (tList, lhs) =
        match tList with
        | Mul :: tail ->
            let (tLst, rhs) = F tail
            Topt (tLst, MulCalculus (lhs, rhs))
        | Div :: tail ->
            let (tLst, rhs) = F tail
            Topt (tLst, DivCalculus (lhs, rhs))
        | _ -> (tList, lhs)

    and F tList =
        let (tLst, lhs) = P tList
        Fopt (tLst, lhs)

    and Fopt (tList, lhs) =
        match tList with
        | Pow :: tail ->
            let (tLst, rhs) = P tail
            Fopt (tLst, PowCalculus (lhs, rhs))
        | _ -> (tList, lhs)

    and P tList =
        match tList with
        | NumInt n :: tail -> (tail, Const (float n))
        | NumFloat n :: tail -> (tail, Const n)
        | Variable v :: tail -> (tail, Var v)
        | Lpar :: tail ->
            let (tLst, expr) = E tail
            match tLst with
            | Rpar :: tail -> (tail, expr)
            | _ -> raise (System.Exception "Parse error: Expected closing parenthesis.")
        | Sub :: tail ->
            let (tLst, expr) = P tail
            (tLst, SubCalculus (Const 0.0, expr))
        | Sin :: tail ->
            let (tLst, expr) = P tail
            (tLst, SinCalculus expr)
        | Cos :: tail ->
            let (tLst, expr) = P tail
            (tLst, CosCalculus expr)
        | Exp :: tail ->
            let (tLst, expr) = P tail
            (tLst, ExpCalculus expr)
        | Log :: tail ->
            let (tLst, expr) = P tail
            (tLst, LogCalculus expr)
        | IntegralStart :: rest ->
            let (rest1, lower) = P rest
            match rest1 with
            | Comma :: rest2 ->
                let (rest3, upper) = P rest2
                match rest3 with
                | Comma :: rest4 ->
                    let (rest5, integrand) = E rest4
                    match rest5 with
                    | Rpar :: rest6 -> (rest6, Integral (lower, upper, integrand))
                    | _ -> failwith "Expected closing parenthesis"
                | _ -> failwith "Expected integrand after upper limit"
            | _ -> failwith "Expected upper limit after lower limit"
        | _ -> raise (System.Exception "Parse error: Invalid token.")

    
    E tList

    // Example usage
let example = Integral (Const 0.0, Const 1.0, PowCalculus (Var "x", Const 2.0))
let result1 = evaluate example "x" 0.0
System.Diagnostics.Debug.WriteLine("Definite integral result:" + result1.ToString())

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

let evaluateCalculus (input: string) : string = 
    let removeFirstSecondAndLast list =
        match list with
        | Derivative "x" :: _ :: tail when tail.Length > 1 -> 
            tail |> List.rev |> List.tail |> List.rev
      
        | _ -> 
            list 

    let tokenList = lexer input
    let modifiedTokenList = removeFirstSecondAndLast tokenList
    System.Diagnostics.Debug.WriteLine("$$$$$Parsed List: " + string modifiedTokenList)

    // Call the parseExpr function and assign the result to a variable
    let (_, parsedExpr) = parseExpr modifiedTokenList
    System.Diagnostics.Debug.WriteLine("$$$$$Parsed List: " + string parsedExpr)

    let derivative = differentiate parsedExpr "x"
    let simplifiedDerivative = simplify derivative
    let result = exprToString simplifiedDerivative

    let formattedResult = result.ToString()

     
    System.Diagnostics.Debug.WriteLine(formattedResult) 

    formattedResult


let evaluateExpression (input: string) : string =
    let statements = splitString ';' input
    let calculusResult = [| "" |]  
    let flag = [| false |]  
    System.Diagnostics.Debug.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@")

    let processStatement (state: State) (statement: string) : State =
        let trimmedStatement = statement.Trim()
        if trimmedStatement <> "" then
            let tokenList = lexer trimmedStatement
            System.Diagnostics.Debug.WriteLine("******Token List: " + string tokenList)

             
           
            match tokenList with
                | Derivative "x" :: Lpar :: _ -> 
                    tokenList 
                    |> List.iter (fun token -> System.Diagnostics.Debug.WriteLine("Token: " + string token))


                    let result = evaluateCalculus trimmedStatement
                    System.Diagnostics.Debug.WriteLine("Result from evaluateCalculus: " + result)
                    calculusResult.[0] <- result  
                    flag.[0] <- true   
                    System.Diagnostics.Debug.WriteLine("Result from evaluateCalculus###: " + calculusResult.[0])
                    { state with lastResult = 0.0 } 
                | IntegralStart :: _ -> 
                    let (_, parsedExpr) = parseExpr tokenList
                    let result = evaluate parsedExpr "x" 0.0
                    calculusResult.[0] <- sprintf "%f" result
                    flag.[0] <- true
                    { state with lastResult = result }
                    
                | _ -> 
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

    if flag.[0] = false then
        formattedResult
    else
        calculusResult.[0]

let example1 = "Integral(0,1,x^2)"
let exampleResult = evaluateExpression example1
System.Diagnostics.Debug.WriteLine("Result of Integral:" + exampleResult)


let evaluateNumericalIntegration (input: string) (lower: float) (upper: float) (interval: float) =
    let tokens = lexer input
    let (_, parsedExpr) = parseExpr tokens

    // Replace Integral with its inner function
    let rec extractInnerFunction expr =
        match expr with
        | Integral (_, _, f) -> f  // Extract the inner function
        | _ -> expr               // Default case for non-Integral expressions

    let innerFunction = extractInnerFunction parsedExpr

    let f x = evaluate innerFunction "x" x
    let step = (upper - lower) / interval
    let xValues = [lower .. step .. upper]
    let yValues = xValues |> List.map f

    // Debugging output
    printfn "xValues: %A" xValues
    printfn "yValues: %A" yValues

    (xValues, yValues)

let evaluateDerivativeAt (input: string) (xValue: float) : float =
    try
        let derivativeExpr = evaluateCalculus input
        System.Diagnostics.Debug.WriteLine($"Computed Derivative Expression: {derivativeExpr}")

        let formattedExpr = Regex.Replace(derivativeExpr, @"(\d)([a-zA-Z])", "$1 * $2")
        System.Diagnostics.Debug.WriteLine($"Formatted Derivative Expression: {formattedExpr}")

        let tokenList = lexer formattedExpr
        System.Diagnostics.Debug.WriteLine($"Derivative Tokens: {tokenList}")

        let (_, parsedExpr) = parseExpr tokenList
        System.Diagnostics.Debug.WriteLine($"Parsed Derivative Expression: {parsedExpr}")

        let result = evaluate parsedExpr "x" xValue
        System.Diagnostics.Debug.WriteLine($"Evaluated Derivative at x={xValue}: {result}")
        result
    with
    | ex ->
        System.Diagnostics.Debug.WriteLine($"Error in evaluateDerivativeAt: {ex.Message}")
        raise ex





//let evaluateCalculus (input: string) : string = 
//    let tokenList = lexer input

//    // Call the parseExpr function and assign the result to a variable
//    let (_, parsedExpr) = parseExpr tokenList
//    System.Diagnostics.Debug.WriteLine("$$$$$Parsed List: " + string parsedExpr)

//    let derivative = differentiate parsedExpr "x"
//    let simplifiedDerivative = simplify derivative
//    let result = exprToString simplifiedDerivative

//    let formattedResult = result.ToString()

     
//    System.Diagnostics.Debug.WriteLine(formattedResult) 

//    formattedResult


let evaluatePolynomial (input: string) : string =
    try
        let trimmedInput = input.Trim()
        System.Diagnostics.Debug.WriteLine(trimmedInput)

        // Check if the input contains 'y', '=', and 'x' (basic validation)
        if not (trimmedInput.Contains("y") && trimmedInput.Contains("=") && trimmedInput.Contains("x")) then
            "Invalid polynomial. Missing 'y', '=', or 'x'."

        // Check if the expression has a valid form: y = [expression with x]
        else
            let tokenList = lexer trimmedInput
            System.Diagnostics.Debug.WriteLine("Token List: " + string tokenList)

            let (parsedList, result) = parseAssignment tokenList
            System.Diagnostics.Debug.WriteLine("Parsed List: " + string parsedList)
            System.Diagnostics.Debug.WriteLine("Result: " + string result)

            validateTokens tokenList parsedList

            // Further check if the structure is correct, like "y = x + 10"
            let equationParts = trimmedInput.Split('=')
            if equationParts.Length <> 2 then
                "Invalid polynomial. Should be in the form 'y = [expression]'."
            else
                let expression = equationParts.[1].Trim()
                if expression.Contains("x") then
                    "Valid polynomial."
                else
                    "Invalid polynomial. The expression should include 'x'."
    with
    | :? System.Exception as ex when ex.Message.StartsWith("Variable 'x'") && ex.Message.Contains("is not defined") ->
        // Handle undefined variable exception
        System.Diagnostics.Debug.WriteLine("Handled undefined variable: " + ex.Message)
        "Valid polynomial." 

    | :? System.Exception as ex ->
        System.Diagnostics.Debug.WriteLine("Error: " + ex.Message)
        "Invalid polynomial. Check your input expression."



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



let splineInterpolation (xValues: float list) (yValues: float list) (queryX: float list) =
    let spline = CubicSpline.InterpolateNatural(xValues |> List.toArray, yValues |> List.toArray)
    queryX |> List.map spline.Interpolate

let evaluateExpressionForCompiler (input: string) : string =
    // Tokenize the input string
    let tokenList = lexer input
    System.Diagnostics.Debug.WriteLine("******Token List: " + string tokenList)
    
    // Parse and evaluate the token list
    let (remaining, result) = parseExpr tokenList
    
    // Format the result
    let formattedResult =
        if initialState.IsFloatDetected then
            result.ToString()
        else
            result.ToString()
    
    System.Diagnostics.Debug.WriteLine("Final Result: " + formattedResult)
    formattedResult

// Generate Python Code
let rec translateToPython (expr: Expr): string =
    match expr with
    | Const c -> c.ToString()
    | Var v -> v
    | AddCalculus (e1, e2) -> sprintf "(%s + %s)" (translateToPython e1) (translateToPython e2)
    | SubCalculus (e1, e2) -> sprintf "(%s - %s)" (translateToPython e1) (translateToPython e2)
    | MulCalculus (e1, e2) -> sprintf "(%s * %s)" (translateToPython e1) (translateToPython e2)
    | DivCalculus (e1, e2) -> sprintf "(%s / %s)" (translateToPython e1) (translateToPython e2)
    | PowCalculus (e, Const n) -> sprintf "(%s ** %s)" (translateToPython e) (translateToPython (Const n))
    | SinCalculus e -> sprintf "math.sin(%s)" (translateToPython e)
    | CosCalculus e -> sprintf "math.cos(%s)" (translateToPython e)
    | LogCalculus e -> sprintf "math.log(%s)" (translateToPython e)
    | _ -> failwith "Unsupported expression for transpilation."

let generatePythonCode (expr: Expr): string =
    let translatedExpr = translateToPython expr
    sprintf """
        import math

        def user_function(x):
            return %s

        if __name__ == "__main__":
            print(user_function(2))  # Example test
        """ translatedExpr

// Compile Python Code
let compilePythonCode (pythonCode: string) (outputPath: string) =
    File.WriteAllText(outputPath, pythonCode)
    let process = Process.Start("python", sprintf "-m py_compile %s" outputPath)
    process.WaitForExit()
    if process.ExitCode <> 0 then
        failwith "Python compilation failed."

// Run Python Code
let runPythonCode (scriptPath: string) =
    let process = new Process()
    process.StartInfo.FileName <- "python"
    process.StartInfo.Arguments <- scriptPath
    process.StartInfo.UseShellExecute <- false
    process.StartInfo.RedirectStandardOutput <- true
    process.Start()
    let output = process.StandardOutput.ReadToEnd()
    process.WaitForExit()
    output


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
