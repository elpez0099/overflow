import {getQuestions} from "@/lib/actions/question-actions";
import QuestionCard from "@/app/questions/QuestionCard";
import QuestionHeader from "@/app/questions/QuestionHeader";

export default async function QuestionsPage({searchParams}: {searchParams?: Promise<{tag?:string}>}) {
    const params = await searchParams;
    const questions = await getQuestions(params?.tag);
    return (
        <>
            <QuestionHeader total={questions.length} tag={params?.tag} />
            {questions.map((question, index) => (
                <div key={question.id} className="py-4 not-last:border-b w-full flex">
                    <QuestionCard key={question.id} question={question} />
                </div>
            ))}
        </>
    );
}