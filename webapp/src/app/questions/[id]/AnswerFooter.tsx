import {Answer} from "@/lib/types";
import {Avatar, Link} from "@heroui/react";

type Props = {
    answer: Answer
}

export default function AnswerFooter({answer}: Props) {
    return (
        <div className='flex justify-end mt-4'>
            <div className="flex flex-col basis-2/5 bg-primary/10 px-3 py-2 gap-2 rounded-lg">
                <span className='text-sm font-extralight'> asked {answer.createdAt}</span>
                <div className="flex gap-3 items-center">
                    <Avatar className="size-6">
                        <Avatar.Fallback>
                            {answer.userDisplayName.charAt(0).toUpperCase()}
                        </Avatar.Fallback>
                    </Avatar>
                    <Link href={`/profiles/${answer.userId}`}>
                        {answer.userDisplayName}
                    </Link>
                    <span className='self-start text-sm  font-semibold'>42</span>
                </div>
            </div>
        </div>
    );
}

