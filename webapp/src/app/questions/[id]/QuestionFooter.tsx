import {Question} from "@/lib/types";
import {Avatar, Chip, Link} from "@heroui/react";

type Props ={
    question: Question
}
export default function QuestionFooter({question}: Props) {
    return (
        <div className="flex justify-between mt-2">
            <div className="flex flex-col self-end">
                <div className="flex gap-2">
                    {question.tagSlugs.map((tag: string) => (
                        <Link href={`/questions?tag=${tag}`} key={tag}>
                            <Chip>
                                {tag}
                            </Chip>
                        </Link>
                    ))}
                </div>
            </div>
            <div className="flex flex-col basis-2/5 bg-primary/10 px-3 py-2 gap-2 rounded-lg">
                <span className='text-sm font-extralight'> asked {question.createdAt}</span>
                <div className="flex gap-3 items-center">
                    <Avatar className="size-6">
                        <Avatar.Fallback>
                            {question.askerDisplayName.charAt(0).toUpperCase()}
                        </Avatar.Fallback>
                    </Avatar>
                    <Link href={`/profiles/${question.askerId}`}>
                        {question.askerDisplayName}
                    </Link>
                    <span className='self-start text-sm  font-semibold'>42</span>
                </div>
            </div>
        </div>
    );
}
