module Task

let ignore t =
    task {
        let! _ = t
        return ()
    }
