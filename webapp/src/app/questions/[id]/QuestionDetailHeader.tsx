import {Question} from "@/lib/types";
import {Button, Link} from "@heroui/react";

type Props = {
    question: Question;
}

export default function QuestionDetailHeader({question}: Props) {
    return (
        <div className='flex flex-col w-full border-b gap-4 pb-4 px-6'>
            <div className='flex justify-between gap-4'>
                <div className='text-3xl font-semibold text-gray-900 first-letter:uppercase'>
                    {question.title}
                </div>
                <Link href='/questions/ask' className='w-[20%]'>
                <Button >
                    Ask question
                </Button>
                </Link>
            </div>
            <div className='flex items-center gap-6'>
                <div className='flex items-center gap-3'>
                    <span className='text-foreground-500'>Asked</span>
                    <span>{question.createdAt}</span>
                </div>
                {question.updatedAt && (
                    <div className='flex items-center gap-3'>
                        <span className='text-foreground-500'>Updated</span>
                        <span>{question.updatedAt}</span>
                    </div>
                )}

                <div className='flex items-center gap-3'>
                    <span className='text-foreground-500'>Viewed</span>
                    <span>{question.viewCount + 1} times</span>
                </div>
            </div>
            
        </div>
    );
}