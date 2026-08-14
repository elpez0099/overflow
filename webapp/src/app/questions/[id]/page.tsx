//  El router envia algo como:
// params: Promise.reoslve({
//         id: "123"
//  })
// Este parametro (params) es pasado como una promesa
// De modo que para extraer el valor de los parametros se debe resolver la promesa
// por eso se usa async
// Typescript obliga a ser explicitos en cuento al tipo de datos, asi que se hace destructuring de el objeto
// que pasa como parametro el router(params) y se define su tipo, que es un objeto json cuyo key es params 
// y su tipo es una promesa que devuelve un objeto json con una propidad id

import {getQuestionsById} from "@/lib/actions/question-actions";
import {notFound} from "next/navigation";
import QuestionDetailHeader from "@/app/questions/[id]/QuestionDetailHeader";
import QuestionContent from "@/app/questions/[id]/QuestionContent";
import AnswerContent from "@/app/questions/[id]/AnswerContent";
import AnswerHeader from "@/app/questions/[id]/AnswerHeader";

export default async function QuestionDetailPage({params}: {params: Promise<{id:string}>}) {
    const {id} = await params;
    const question = await getQuestionsById(id);
    
    if(!question) return notFound();
    
    return (
        <div className='w-full'>
            <QuestionDetailHeader question={question}/>
            <QuestionContent question={question}/>
            {question.answers.length > 0 && (
                <AnswerHeader answerCount={question.answers.length}/>
            )}
            {question.answers && (
                question.answers.map((answer) => (
                    <AnswerContent key={answer.id} answer={answer} />    
                ))
                
            )}
        </div>
    );
}