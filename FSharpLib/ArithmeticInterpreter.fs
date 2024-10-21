module ArithmeticInterpreter

open System


type terminal = 
    | Add | Sub | Mul | Div | Mod | Pow | Lpar | Rpar | Num of int | Invalid of char

let str2lst s = [for c in s -> c]
let isblank c = System.Char.IsWhiteSpace c
let isdigit c = System.Char.IsDigit c
let lexError = System.Exception("Lexer error")
let intVal (c:char) = (int)((int)c - (int)'0')
let parseError = System.Exception("Parser error")

let rec scInt(iStr, iVal) = 
    match iStr with
    | c :: tail when isdigit c -> scInt(tail, 10 * iVal + intVal c)
    | _ -> (iStr, iVal)


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
        | c :: tail when isblank c -> scan tail
        | c :: tail when isdigit c -> 
            let (iStr, iVal) = scInt(tail, intVal c)
            Num iVal :: scan iStr
        | c :: tail -> Invalid c :: scan tail 
    scan (str2lst input)


let rec parseNeval tList = 
    let rec E tList = (T >> Eopt) tList
    and Eopt (tList, value) = 
        match tList with
        | Add :: tail -> let (tLst, tval) = T tail
                         Eopt (tLst, value + tval)
        | Sub :: tail -> let (tLst, tval) = T tail
                         Eopt (tLst, value - tval)
        | _ -> (tList, value)
    and T tList = (NR >> Topt) tList
    and Topt (tList, value) =
        match tList with
        | Mul :: tail -> let (tLst, tval) = NR tail
                         Topt (tLst, value * tval)
        | Div :: tail -> let (tLst, tval) = NR tail
                         Topt (tLst, value / tval)
        | Mod :: tail -> let (tLst, tval) = NR tail
                         Topt (tLst, value % tval)
        | Pow :: tail -> let (tLst, tval) = NR tail
                         Topt (tLst, int (float value ** float tval)) 
        | _ -> (tList, value)
    and NR tList =
        match tList with 
        | Num value :: tail -> (tail, value)
        | Lpar :: tail -> 
            let (tLst, tval) = E tail
            match tLst with 
            | Rpar :: tail -> (tail, tval)
            | _ -> raise parseError  
        | Sub :: tail -> 
            let (tLst, tval) = NR tail 
            (tLst, -tval)  
        | _ -> raise parseError  
    E tList


let validateTokens tokenList parsedList =
    match parsedList with
    | [] -> ()  
    | Invalid c :: _ -> raise (System.Exception(sprintf "Invalid character: '%c'" c))
    | _ -> raise parseError  


let evaluateExpression input =
    let tokenList = lexer input
    let parsedList, result = parseNeval tokenList
    validateTokens tokenList parsedList
    result